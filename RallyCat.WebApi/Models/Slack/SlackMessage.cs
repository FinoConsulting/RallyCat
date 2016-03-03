using System;

namespace RallyCat.WebApi.Models.Slack
{
    public class SlackMessage
    {
        public String           ChannelId   { get; set; }
        public String           ChannelName { get; set; }
        public String           Command     { get; set; }
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
            }

            return msg;
        }
    }
}