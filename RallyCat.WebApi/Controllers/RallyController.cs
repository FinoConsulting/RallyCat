
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
using System.Threading;
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
        private readonly AzureService _AzureService;
        private readonly GraphicService _GraphicService;
        private readonly RallyService _RallyService;

        public RallyController()
        {
            RallyCatDbContext.SetConnectionString("RallyCatConnection");

            var dbContext   = RallyCatDbContext.QueryDb();
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

            // todo: this belongs somewhere else, maybe as a constant in a RallyHelper.cs?
            var detailRegex      = @"((US|Us|uS|us)\d{1,9})|(((dE|de|De|DE)\d{1,9}))";
            var regex            = new Regex(detailRegex);
            var slackMessageText = msg.Text.ToLower();

            if (slackMessageText.Contains("help")) { return new SlackResponseVm(GetHelpMsg()); }

            var pattern          = '+';
            var slackText        = slackMessageText.Split(pattern);
            var channel          = msg.ChannelName;
            var result           = ":warning: Something went wrong. Type /rallycat help for available commands.";
            var responseUrl      = msg.ResponseUrl;

            if (responseUrl != null)
            {
                // temporary auto-response 
                string autoResponse         = "{\"text\":\"Ignore the time out error!!!.... trust me...\"}";
                SendMessageToSlack(responseUrl, autoResponse);
            }

            var isStoryOrDefect = regex.Match(msg.Text);
            // todo: check match.Groups for null
            var formattedId     = isStoryOrDefect.Groups[0].Value;

            foreach (var element in slackText)
            {
                if (element.Contains("kanban") || element.Contains("rallycat") || regex.IsMatch(element)) { continue; }
                channel = element;
            }

            if (isStoryOrDefect.Success) { result = (responseUrl == null) ? GetItem(formattedId, channel) : GetItemWithAttachments(formattedId, channel); }
    
            if (slackMessageText.Contains("kanban")) { result = GetKanban(channel); }

            if (responseUrl != null)
            {
                string postData = (slackMessageText.Contains("kanban"))
                    ? "{\"attachments\":[{\"text\":\"" + result + "\",\"image_url\":\"" + result + "\"}]}"
                    : "{\"text\":\"Result(s):\", \"attachments\": [{\"text\":\"" + result + "\"}]}"; 
            //{
            //    string postData = "{\"text\":\"Result(s):\", \"attachments\": [{\"text\":\"" + result + "\"}]}";

            //    if (slackMessageText.Contains("kanban"))
            //    {
            //        postData = "{\"attachments\":[{\"text\":\"" + result + "\",\"image_url\":\"" + result + "\"}]}";
            //    }

                SendMessageToSlack(responseUrl, postData);
                return new SlackResponseVm("tadaaaa!");
            }
            return new SlackResponseVm(result);
        }

        private void SendMessageToSlack(string responseUrl, string postData)
        {
            var formattedUrl = Regex.Replace(responseUrl, "%2F", "/");
            var postUrl = Regex.Replace(formattedUrl, "%3A", ":");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);

            var data = Encoding.ASCII.GetBytes(postData);
            request.ProtocolVersion = HttpVersion.Version11;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
        }

        [System.Web.Mvc.Route("api/Rally/Kanban/{channelName}")]
        public String GetKanban(String channelName)
        {
            // get current Slack channel
            var mappings        = RallyBackgroundDataService.Instance.RallySlackMappings;
            var map             = mappings.Find(o => o.Channels.Contains(channelName.ToLower()));
            if (map == null) { return ":no_entry: Project: " + channelName + " not found. Please try again."; }

            // get kanban for this channel
            var result          = _RallyService.GetKanban(map);
            if (result == null) { return ":no_entry: Kanban for " + channelName + " not found. Please try again."; }

            // sort items, then draw kanban

            var kanbanItems     = result.Select(o => KanbanItem.ConvertFrom(o, map.KanbanSortColumn)).Cast<KanbanItem>();
            var kanbanColumns   = _RallyService.GetOrderedColumns(map);

            foreach (var item in kanbanItems.GroupBy(k => k.KanbanState))
            {
                if (kanbanColumns.ContainsKey(item.Key))
                {
                    kanbanColumns[item.Key] = item.OrderBy(t => t.AssignedTo).ToList();
                }
            }
            var img = _GraphicService.DrawWholeKanban(500, 20, 20, 20, 100, kanbanColumns);
            return _AzureService.Upload(img, string.Format("{0}-kanban", channelName));
        }
        public String GetItem(String formattedId, String channelName)
        {
            var mappings        = RallyBackgroundDataService.Instance.RallySlackMappings;
            var map             = mappings.Find(o => o.Channels.Contains(channelName.ToLower()));
            if (map == null) { return ":no_entry: Project: " + channelName + " not found."; }

            var result          = _RallyService.GetRallyItemById(map, formattedId);
            var item            = result.Results.FirstOrDefault();

            if (item == null) { return ":no_entry: User Story/Defect: " + formattedId + " not found"; }

            var itemName        = (String)item["Name"];
            var itemDescription = ((String)item["Description"]).HtmlToPlainText();
            return String.Format("_{0}_:smirk_cat:" + "\r\n\r\n\r\n\r\n" + "*{1}*\r\n" + ">>>\r\n{2}", "I've been summoned!!", itemName, itemDescription);
        }

        public String GetItemWithAttachments(String formattedId, String channelName)
        {
            var mappings          = RallyBackgroundDataService.Instance.RallySlackMappings;
            var map               = mappings.Find(o => o.Channels.Contains(channelName.ToLower()));

            if (map == null) { return ":no_entry: Project: " + channelName + " not found. Please try again.\", \"color\": \"#ff0033"; }
            var result            = _RallyService.GetRallyItemById(map, formattedId);
            var item              = result.Results.FirstOrDefault();

            if (item == null) { return ":no_entry: User Story/Defect: " + formattedId + " not found.\", \"color\": \"#ff0033"; }
            var userStoryOrDefect = KanbanItem.ConvertFrom(item, map.KanbanSortColumn);
            var fullDescription   = userStoryOrDefect.FullDescription.Replace("\"", "\\\"");
            var storyDescription  = userStoryOrDefect.StoryDescription.Replace("\"", "\\\"");
            userStoryOrDefect = fullDescription + "\", \"author_name\":\"" + storyDescription.ToUpper() + "\", \"color\":\""
             + userStoryOrDefect.DisplayColor + "\", \"fields\": [{\"title\":\"Assigned To\", \"value\":\"" + userStoryOrDefect.AssignedTo + "\",\"short\": true }], \"title\":\"" + userStoryOrDefect.FormattedId;
            return userStoryOrDefect;

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
            var r = new Random((int)DateTime.Now.Ticks);
            return welcomes[r.Next(0, welcomes.Count - 1)];
        }

        private static string GetHelpMsg()
        {
            return @"---*Available Commands*---

--From any channel (private and public): 

```/rallycat [project name] kanban
/rallycat [project name] us#### 
/rallycat [project name] de####```

 --From any public channel: 

```rallycat: [project name] kanban
rallycat: [project name] us####
rallycat: [project name] de####```

--From specific project channel:
 ```rallycat: kanban 
rallycat: us####
rallycat: de####```

";
        }
    }

}

