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

        //public RallyApi.RallyRestApi GetApi(string userName, string password)
        //{
        //    int hash = (userName + "|" + password).GetHashCode();
        //    if (Pools == null)
        //    {
        //        Pools = new Dictionary<int, RallyApi.RallyRestApi>();
        //    }
        //    if (Pools.ContainsKey(hash) && Pools[hash].ConnectionInfo.UserName == userName && Pools[hash].ConnectionInfo.Password == password)
        //        {
        //            var conn = Pools[hash];
        //            if (conn.ConnectionInfo != null &&
        //                conn.AuthenticationState == RallyApi.RallyRestApi.AuthenticationResult.Authenticated)
        //            {
        //                return conn;
        //            }
        //            RallyApi.RallyRestApi.AuthenticationResult r = conn.Authenticate(userName, password, allowSSO:false);
        //            if (r != RallyApi.RallyRestApi.AuthenticationResult.Authenticated)
        //            {
        //                return null;
        //            }
        //        }

        //        RallyApi.RallyRestApi api = new RallyApi.RallyRestApi();
        //        var result = api.Authenticate(userName, password, allowSSO:false);
        //        if (result != RallyApi.RallyRestApi.AuthenticationResult.Authenticated)
        //        {
        //            return null;
        //        }
        //        Pools.Add(hash, api);
        //        return api;
        //    }

        // getapi with token 

        public RallyApi.RallyRestApi GetApi(string apiKey, string rallyServer)
        {
            RallyApi.RallyRestApi api = new RallyApi.RallyRestApi();
            var result = api.AuthenticateWithApiKey("_LrqfvZJdQ8O2UtDHia4uxpNpFBaZUFcoVuEWLpxJy0",
                "https://rally1.rallydev.com");
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
