using System;

namespace RallyCat.WebApi.Models.Slack
{
    public enum SlackMessageType
    {
        OutgoingWebhooks,
        SlashCommand
    }

    public class SlackMessage
    {
        public string Token { get; set; }
        public string TeamId { get; set; }
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string TeamDomain { get; set; }
        public DateTime TimeStamp { get; set; }
        public string UserId { get; set; }
        public string ServiceId { get; set; }
        public string UserName { get; set; }
        public string TriggerWord { get; set; }
        public string Text { get; set; }
        //For Slack Command
        public string Command { get; set; }

        //Type
        public SlackMessageType MessageType { get; set; }


        public static SlackMessage FromString(string source)
        {
            //token=zvhpiUW4HnFUACA1lPFZuQXO&team_id=T024SS9SJ&team_domain=finoconsulting&service_id=2539062323&channel_id=C029YKXUP&channel_name=slacktesting&timestamp=1410311282.000007&user_id=U024T9P3E&user_name=cheng.huang&text=rallybot%3A+US1243&trigger_word=rallybot%3A 
            var msg = new SlackMessage();
            var entries = source.Split('&');
            foreach (var en in entries)
            {
                var element = en.Split('=');

                if (en.Contains("token"       )) { msg.Token       = element[1];                                             }
                if (en.Contains("team_id"     )) { msg.TeamId      = element[1];                                             }
                if (en.Contains("team_domain" )) { msg.TeamDomain  = element[1];                                             }
                if (en.Contains("service_id"  )) { msg.ServiceId   = element[1];                                             }
                if (en.Contains("channel_id"  )) { msg.ChannelId   = element[1];                                             }
                if (en.Contains("channel_name")) { msg.ChannelName = element[1];                                             }
                if (en.Contains("timestamp"   )) { msg.TimeStamp   = Convert.ToDouble(element[1]).UnixTimeStampToDateTime(); }
                if (en.Contains("user_id"     )) { msg.UserId      = element[1];                                             }
                if (en.Contains("user_name"   )) { msg.UserName    = element[1];                                             }
                if (en.Contains("trigger_word")) { msg.TriggerWord = element[1];                                             }
                if (en.Contains("text"        )) { msg.Text        = element[1];                                             }
                if (en.Contains("command"     )) { msg.Command     = element[1];                                             }
            }

            return msg;
        }
    }
}