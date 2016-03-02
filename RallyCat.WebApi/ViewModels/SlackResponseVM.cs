using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace RallyCat.WebApi.ViewModels
{
    public class SlackResponseVM
    {
        public SlackResponseVM()
        {
            
        }

        public SlackResponseVM(string responseText)
        {
            text = responseText;
            // response_url = responseUrl;
        }
        public string text { get; set; }
        // public string response_url { get; set; }
    }
}