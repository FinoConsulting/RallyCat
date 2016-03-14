using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;


namespace RallyCat.WebApi.ViewModels
{
    public class SlackResponseVm
    {
        public SlackResponseVm() { }

        public SlackResponseVm(String responseText) { text = responseText; }

        // todo: add json names for these properties
        public String text { get; set; }
    }
}