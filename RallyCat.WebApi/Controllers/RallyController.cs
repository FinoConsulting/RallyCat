using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using FluentData;
using Microsoft.SqlServer.Server;
using RallyCat.Core;
using RallyCat.Core.DataAccess;
using RallyCat.Core.Rally;
using RallyCat.Core.Services;
using RallyCat.WebApi.Models.Slack;
using RallyCat.WebApi.ViewModels;

namespace RallyCat.WebApi.Controllers
{
    public class RallyController : ApiController
    {
        //
        // GET: /Rally/
        public IDbContext _dbContext;
        private RallyService _rallyService;
        private GraphicService _graphicService;
        private AzureService _azureService;
        public RallyController()
        {
            RallyCatDbContext.SetConnectionString("RallyCatConnection");
            _dbContext = RallyCatDbContext.QueryDb();
            RallyBackgroundData.SetDbContext(_dbContext);
            _rallyService = new RallyService(RallyBackgroundData.Instance);
            _graphicService = new GraphicService();
            _azureService = new AzureService(RallyBackgroundData.Instance);
        }

        [Route("api/Rally/Details")]
        [HttpPost]
        public async Task<SlackResponseVM> Details()
        {
            string input = await Request.Content.ReadAsStringAsync();

            string str = input;

            SlackMessage msg = SlackMessage.FromString(str);
            msg.MessageType = SlackMessageType.OutgoingWebhooks;
            Regex regex = new Regex(@"((US|Us|uS|us)\d{1,9})|(((dE|de|De|DE)\d{1,9}))");
            Match m = regex.Match(msg.Text);

            if (!m.Success)
            {
                if (msg.Text.ToLower().Contains("kanban"))
                {
                    String slackMessageText = msg.Text.ToLower();
                    Char pattern = '+';
                    String[] slackText = slackMessageText.Split(pattern);
                    if (slackText.Length > 2)
                    {
                        foreach (string element in slackText)
                        {
                            if (!(element.Contains("kanban") || element.Contains("rallycat")))
                            {
                                String channel = element;
                                //return new SlackResponseVM(channel);
                                return new SlackResponseVM(GetKanban(channel));
                            }
                        }
                    }
                    else
                    {
                        return new SlackResponseVM(GetKanban(msg.ChannelName));
                    }
                }
                return new SlackResponseVM("_Whuaaat?_" );
            }

            string formattedId = m.Groups[0].Value;
            string result = GetItem(formattedId, msg.ChannelName);
            return new SlackResponseVM (result);
          
        }

        [Route("api/Rally/Kanban/{channelName}")]
        public string GetKanban(string channelName)
        {
            var mappings = RallyBackgroundData.Instance.RallySlackMappings;
            var map = mappings.Find(o => o.Channels.Contains(channelName.ToLower()));
            if (map == null)
            {
                throw new ObjectNotFoundException("Cannot found channel name mapping for " + channelName);
            }
            
            var result = _rallyService.GetKanban(map);
            var list = result;
            if (list == null)
            {
                return null;
            }
            //requestFields = new List<string>()
            //{
            //    "Name",
            //    "ObjectID",
            //    "FormattedID",
            //    "Description",
            //    "ScheduleState",
            //    "Owner"
            //};
            var kanbanGroup = list.Select(o=> KanbanItem.ConvertFrom(o,map.KanbanSortColumn)).Cast<KanbanItem>().GroupBy(k=>k.KanbanState).ToDictionary(k=>k.Key, o=>o.OrderBy(t=>t.AssignedTo).ToList());
            Image img = _graphicService.DrawWholeKanban(500, 20, 20, 20, 100, kanbanGroup);
            var uploaded = _azureService.Upload(img, string.Format("{0}-kanban", channelName));
            return uploaded;
        }

        public string GetItem(string formattedId, string channelName)
        {
            
            if (formattedId.StartsWith("DE", StringComparison.InvariantCultureIgnoreCase))
            {
                
            }
            var mappings = RallyBackgroundData.Instance.RallySlackMappings;
            var map = mappings.Find(o => o.Channels.Contains(channelName.ToLower()));
            if (map == null)
            {
                throw new ObjectNotFoundException("Cannot found channel name mapping for " + channelName);
            }
            
            var result = _rallyService.GetRallyItemById(map, formattedId);
            var item = result.Results.FirstOrDefault();
            if (item == null)
            {
                return null;
            }
            string itemName = (string)item["Name"];
            string itemDescription = ((string)item["Description"]).HtmlToPlainText();
            return "_" + GetWelcomeMsg() + "_" + "\r\n\r\n" + "*" + itemName.ToUpper() + "*\r\n" + "*" + itemName + "*" + "\r\n" + itemDescription;

        }





        private string GetWelcomeMsg()
        {
            List<string> welcomes = new List<string> { "how can I help all of you slackers?", "you called?", "Wassup?", "I think I heard my name", "Yes?", "At your service" };
            Random r = new Random((int)DateTime.Now.Ticks);
            return welcomes[r.Next(0, welcomes.Count - 1)];
        }
    }
}
