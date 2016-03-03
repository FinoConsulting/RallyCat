using System;
using System.Collections.Generic;
using RallyApi = Rally.RestApi;

namespace RallyCat.Core.Rally
{
    public class RallyApiConnectionPool
    {
        public const Int32 MaxConnection = 50;

        public Dictionary<Int32, RallyApi.RallyRestApi> Pools { get; set; }

        public RallyApi.RallyRestApi GetApi(String apiKey, String rallyServer)
        {
            var api        = new RallyApi.RallyRestApi();
            var authStatus = api.AuthenticateWithApiKey(apiKey, rallyServer);
            return authStatus == RallyApi.RallyRestApi.AuthenticationResult.Authenticated ? api : null;
        }
    }
}