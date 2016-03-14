using System;

namespace RallyCat.WebApi.Models.Slack
{
    public class SlackMessage
    {
<<<<<<< HEAD
        public String           ChannelId   { get; set; }
        public String           ChannelName { get; set; }
        public String           Command     { get; set; }
=======
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
        public string ResponseUrl { get; set; }

        //Type
>>>>>>> rallycat-slash-command
        public SlackMessageType MessageType { get; set; }
        public String           ServiceId   { get; set; }
        public String           TeamDomain  { get; set; }
        public String           TeamId      { get; set; }
        public String           Text        { get; set; }
        public DateTime         TimeStamp   { get; set; }
        public String           Token       { get; set; }
        public String           TriggerWord { get; set; }
        public String           UserId      { get; set; }
        public String           UserName    { get; set; }

        public static SlackMessage FromString(String queryString)
        {
            var msg = new SlackMessage();
            foreach (var pair in queryString.Split('&'))
            {
                var key   = pair.Split('=')[0];
                var value = pair.Split('=')[1];

<<<<<<< HEAD
                switch (key) { 
                    case "channel_id"  : msg.ChannelId   = value;                                             break;
                    case "channel_name": msg.ChannelName = value;                                             break;
                    case "command"     : msg.Command     = value;                                             break;
                    case "service_id"  : msg.ServiceId   = value;                                             break;
                    case "team_domain" : msg.TeamDomain  = value;                                             break;
                    case "team_id"     : msg.TeamId      = value;                                             break;
                    case "text"        : msg.Text        = value;                                             break;
                    case "timestamp"   : msg.TimeStamp   = Convert.ToDouble(value).UnixTimeStampToDateTime(); break;
                    case "token"       : msg.Token       = value;                                             break;
                    case "trigger_word": msg.TriggerWord = value;                                             break;
                    case "user_id"     : msg.UserId      = value;                                             break;
                    case "user_name"   : msg.UserName    = value;                                             break;
                }
=======
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
                if (en.Contains("response_url")) { msg.ResponseUrl = element[1];                                             }

>>>>>>> rallycat-slash-command
            }

            return msg;
        }
    }
}