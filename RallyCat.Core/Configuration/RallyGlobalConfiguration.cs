using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyCat.Core.Configuration
{
    public class RallyGlobalConfiguration
    {
        public Boolean EnableGoogleSearch    { get; set; }
        public String  AzureBlobContainerRef { get; set; }
        public String  AzureBlobName         { get; set; }
        public String  AzureToken            { get; set; }
        public String  ErrorSlackResponse    { get; set; }
        public String  KanbanImageFormat     { get; set; }
        public String  NoResultSlackResponse { get; set; }
        public String  RallyToken            { get; set; }
        public String  RallyUrl              { get; set; }
        public String  SlackUrl              { get; set; }
    }
}
