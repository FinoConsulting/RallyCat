using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.WebPages;
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
        private readonly AzureService   _azureService;
        private readonly GraphicService _graphicService;
        private readonly RallyService   _rallyService;

        public RallyController()
        {
            RallyCatDbContext.SetConnectionString("RallyCatConnection");

            var dbContext = RallyCatDbContext.QueryDb();
            RallyBackgroundData.SetDbContext(dbContext);

            _rallyService = new RallyService(RallyBackgroundData.Instance);
            _graphicService = new GraphicService();
            _azureService = new AzureService(RallyBackgroundData.Instance);
        }

        [Route("api/Rally/Details")]
        [HttpPost]
        public async Task<SlackResponseVM> Details()
        {
            var input = await Request.Content.ReadAsStringAsync();
            var msg = SlackMessage.FromString(input);
            msg.MessageType = SlackMessageType.OutgoingWebhooks;
            var regex = new Regex(@"((US|Us|uS|us)\d{1,9})|(((dE|de|De|DE)\d{1,9}))");
            var m = regex.Match(msg.Text);
            var formattedId = m.Groups[0].Value;
            var slackMessageText = msg.Text.ToLower();

            if (slackMessageText.Contains("help"))
            {
                return new SlackResponseVM(GetHelpMsg());
            }

            var pattern = '+';
            var slackText = slackMessageText.Split(pattern);
            var channel = msg.ChannelName;
            var result = GetHelpMsg();
            var responseUrl = msg.ResponseUrl;
            
           
            foreach (var element in slackText)
            {
                if (!(element.Contains("kanban") || element.Contains("rallycat") || regex.IsMatch(element)))
                {
                    channel = element;
                }
            }
            if (m.Success)
            {
                result = GetItem(formattedId, channel);
            }
            if (slackMessageText.Contains("kanban"))
            {
                result = GetKanban(channel);
            }

            if (responseUrl != null)
            {
                var formattedUrl = Regex.Replace(responseUrl, "%2F", "/");
                var postUrl = Regex.Replace(formattedUrl, "%3A", ":");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);
                Encoding encoding = new UTF8Encoding();
                string postData = "{\"text\":\"" + result + "\"}";
                byte[] data = encoding.GetBytes(postData);

                request.ProtocolVersion = HttpVersion.Version11;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;

                Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
                return new SlackResponseVM("Meeeooowww");
            }
            return new SlackResponseVM(result);
        }

        [Route("api/Rally/Kanban/{channelName}")]
        public string GetKanban(string channelName)
        {
            var mappings = RallyBackgroundData.Instance.RallySlackMappings;
            var map = mappings.Find(o => o.Channels.Contains(channelName.ToLower()));
            if (map == null)
            {
                return GetHelpMsg();
                // throw new ObjectNotFoundException("Cannot found channel name mapping for " + channelName);
            }

            var result = _rallyService.GetKanban(map);
            if (result == null)
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
            var kanbanGroup = new Dictionary<string, List<KanbanItem>>();
            var kanbanItems = result.Select(o => KanbanItem.ConvertFrom(o, map.KanbanSortColumn)).Cast<KanbanItem>();

            foreach (var item in kanbanItems.GroupBy(k => k.KanbanState))
            {
                kanbanGroup.Add(item.Key, item.OrderBy(t => t.AssignedTo).ToList());
            }

            var img = _graphicService.DrawWholeKanban(500, 20, 20, 20, 100, kanbanGroup);
            return _azureService.Upload(img, string.Format("{0}-kanban", channelName));
        }

        public string GetItem(string formattedId, string channelName)
        {
            if (formattedId.StartsWith("DE", StringComparison.InvariantCultureIgnoreCase))
            {
                // todo: used?
            }

            var mappings = RallyBackgroundData.Instance.RallySlackMappings;
            var map = mappings.Find(o => o.Channels.Contains(channelName.ToLower()));

            if (map == null) { throw new ObjectNotFoundException("Cannot found channel name mapping for " + channelName); }

            var result = _rallyService.GetRallyItemById(map, formattedId);
            var item   = result.Results.FirstOrDefault();

            if (item == null) { return null; }

            var itemName        = (string)item["Name"];
            var itemDescription = ((string) item["Description"]).HtmlToPlainText();
            return String.Format("_{0}_" + "\r\n\r\n" + "*{1}*\r\n" + "*{2}*" + "\r\n{3}", GetWelcomeMsg(), itemName.ToUpper(), itemName, itemDescription);
        }


        private static string GetWelcomeMsg()
        {
            var welcomes = new List<string>
            {
                "how can I help all of you slackers?",
                "you called?",
                "Wassup?",
                "I think I heard my name",
                "Yes?",
                "At your service"
            };
            var r = new Random((int) DateTime.Now.Ticks);
            return welcomes[r.Next(0, welcomes.Count - 1)];
        }

        private static string GetHelpMsg()
        {
            return "---Available Commands---\r\n\r\n--From any channel (private and public): \r\n\r\n/rallycat [project name] kanban\r\n /rallycat[project name] us#### \r\n /rallycat [project name] de#### \r\n\r\n --From any public channel: \r\n\r\nrallycat: [project name] kanban\r\n rallycat: [project name] us####\r\n rallycat: [project name] de#### \r\n\r\n--From specific project channel:\r\n rallycat: kanban \r\nrallycat: us####\r\nrallycat: de####\r\n\r\n";
        }
    }

}
