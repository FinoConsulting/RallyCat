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

        public SlackResponseVM(string t)
        {
            text = t;
        }
        public string text { get; set; }
    }
}