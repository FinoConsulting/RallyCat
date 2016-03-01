using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RallyCat.Core.Configuration;
using RallyCat.Core.Services;
using RallyApi = Rally.RestApi;
namespace RallyCat.Core.Rally
{
    public class RallyApiConnectionPool
    {
        public const int MaxConnection = 50;
        public Dictionary<int, RallyApi.RallyRestApi> Pools { get; set; }
   
        public RallyApi.RallyRestApi GetApi(string apiKey, string rallyServer)
        {
            RallyApi.RallyRestApi api = new RallyApi.RallyRestApi();
            var result = api.AuthenticateWithApiKey(apiKey, rallyServer);
            if (result == RallyApi.RallyRestApi.AuthenticationResult.Authenticated)
            {
                return api;
            }
            else
            {
                return null;
            }
        }

    }
    
}
