using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
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
        private readonly AzureService   _AzureService;
        private readonly GraphicService _GraphicService;
        private readonly RallyService   _RallyService;

        public RallyController()
        {
            RallyCatDbContext.SetConnectionString("RallyCatConnection");

            var dbContext = RallyCatDbContext.QueryDb();
            RallyBackgroundDataService.SetDbContext(dbContext);

            _AzureService   = new AzureService(RallyBackgroundDataService.Instance);
            _GraphicService = new GraphicService();
            _RallyService   = new RallyService(RallyBackgroundDataService.Instance);
        }

        [System.Web.Mvc.Route("api/Rally/Details")]
        [System.Web.Mvc.HttpPost]
        public async Task<SlackResponseVm> Details()
        {
            var input            = await Request.Content.ReadAsStringAsync();
            var msg              = SlackMessage.FromString(input);
            msg.MessageType      = SlackMessageType.OutgoingWebhooks;

            var slackMessageText = msg.Text.ToLower();
            var pattern          = '+';
            var slackText        = slackMessageText.Split(pattern);
            var channel          = msg.ChannelName;

            // todo: this belongs somewhere else, maybe as a constant in a RallyHelper.cs?
            var regex = new Regex(@"((US|Us|uS|us)\d{1,9})|(((dE|de|De|DE)\d{1,9}))");

            if (slackText.Length > 2)
            {
                foreach (var element in slackText)
                {
                    if (element.Contains("kanban") || element.Contains("rallycat") || regex.IsMatch(element)) { continue; }

                    channel = element;
                }
            }

            var isStoryOrDefect = regex.Match(msg.Text);
            // todo: check m.Groups for null
            var formattedId = isStoryOrDefect.Groups[0].Value;

            if (isStoryOrDefect.Success)
            {
                var item = GetItem(formattedId, channel);
                return new SlackResponseVm(item);
            }

           var kanban = slackMessageText.Contains("kanban")
                ? GetKanban(channel)
                : "Type [ProjectName] kanban OR [ProjectName] [US1234]/[DE1234]";
           return new SlackResponseVm(kanban);
        }

        [System.Web.Mvc.Route("api/Rally/Kanban/{channelName}")]
        public String GetKanban(String channelName)
        {
            // get current Slack channel
            var mappings = RallyBackgroundDataService.Instance.RallySlackMappings;
            var map = mappings.Find(o => o.Channels.Contains(channelName.ToLower()));
            if (map == null) { throw new ObjectNotFoundException(); }

            // get kanban for this channel
            var result = _RallyService.GetKanban(map);
            if (result == null) { return null; }
           
            // sort items, then draw kanban
            var kanbanGroup = new Dictionary<String, List<KanbanItem>>();
            var kanbanItems = result.Select(o => KanbanItem.ConvertFrom(o, map.KanbanSortColumn)).Cast<KanbanItem>();

            foreach (var item in kanbanItems.GroupBy(k => k.KanbanState))
            {
                kanbanGroup.Add(item.Key, item.OrderBy(t => t.AssignedTo).ToList());
            }

            var img = _GraphicService.DrawWholeKanban(500, 20, 20, 20, 100, kanbanGroup);
            return _AzureService.Upload(img, String.Format("{0}-kanban", channelName));
        }

        public String GetItem(String formattedId, String channelName)
        {
            if (formattedId.StartsWith("DE", StringComparison.InvariantCultureIgnoreCase))
            {
                // todo: used?
            }

            var mappings = RallyBackgroundDataService.Instance.RallySlackMappings;
            var map = mappings.Find(o => o.Channels.Contains(channelName.ToLower()));

            if (map == null) { throw new ObjectNotFoundException("Cannot found channel name mapping for " + channelName); }

            var result = _RallyService.GetRallyItemById(map, formattedId);
            var item   = result.Results.FirstOrDefault();

            if (item == null) { return null; }

            var itemName        = (String)item["Name"];
            var itemDescription = ((String) item["Description"]).HtmlToPlainText();
            return String.Format("_{0}_" + "\r\n\r\n" + "*{1}*\r\n" + "*{2}*" + "\r\n{3}", GetWelcomeMsg(), itemName.ToUpper(), itemName, itemDescription);
        }

        private static String GetWelcomeMsg()
        {
            var welcomes = new List<String>
            {
                "how can I help all of you slackers?",
                "you called?",
                "Wassup?",
                "I think I heard my name",
                "Yes?",
                "At your service"
            };
            var r = new Random((Int32) DateTime.Now.Ticks);
            return welcomes[r.Next(0, welcomes.Count - 1)];
        }
    }
}