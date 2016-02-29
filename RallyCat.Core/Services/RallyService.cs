using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using FluentData;
using Rally.RestApi;
using Rally.RestApi.Response;
using RallyCat.Core.DataAccess;
using RallyCat.Core.Rally;
using RallyApi = Rally.RestApi;
namespace RallyCat.Core.Services
{
    public class RallyService
    {
        private RallyApiConnectionPool _pool;
        private RallyBackgroundData _rallyBackgroundData;

        public RallyService(RallyBackgroundData backgroundData)
        {
            _pool = new RallyApiConnectionPool();
            _rallyBackgroundData = backgroundData;
        }

        

        public QueryResult GetRallyItemById(RallySlackMapping map, string formattedId)
        {
            var queryType = "hierarchicalrequirement";
            if (formattedId.StartsWith("de", StringComparison.InvariantCultureIgnoreCase))
            {
                queryType = "defect";
            }
            var query = new Query("FormattedID", Query.Operator.Equals, formattedId);
            var requestFields = new List<string>() { "Name", "Description", "FormattedID" };
            return GetRallyItemByQuery( map, requestFields, query,queryType);
            

        }

        public QueryResult GetRallyItemByQuery(RallySlackMapping map, List<string> requestFields, Query query, string artifectName ="")
        {
            // todo: 
            var rallyToken = "_LrqfvZJdQ8O2UtDHia4uxpNpFBaZUFcoVuEWLpxJy0";
            var rallyUrl = "https://rally1.rallydev.com";
            var api = _pool.GetApi(rallyToken, rallyUrl);
            if (api == null)
            {
                throw new AuthenticationException("Cannot verify rally login");
            }
            Request request;
            if (string.IsNullOrEmpty(artifectName))
            {
                request = new Request();
            }
            else
            {
                request = new Request(artifectName);
            }
            request.Project = "/project/" + map.ProjectId;
            request.Workspace = "/workspace/" + map.WorkspaceId;
            //request.Fetch = new List<string>() { "Name", "Description", "FormattedID" };
            request.Fetch = requestFields;
            request.Query = query;
            QueryResult queryResult = api.Query(request);
            return queryResult;
        }

        public List<dynamic> GetKanban(RallySlackMapping map)
        {
            var rallyToken = "_LrqfvZJdQ8O2UtDHia4uxpNpFBaZUFcoVuEWLpxJy0";
            var rallyUrl = "https://rally1.rallydev.com";
            RallyRestApi restApi = _pool.GetApi(rallyToken, rallyUrl);
            var queryType = "Iteration";
            var query = new Query(string.Format("(StartDate <= {0})", DateTime.Now.ToString("o")));
            var requestFields = new List<string>() { "Name", "StartDate", "Project", "EndDate" };
            QueryResult queryResult = GetRallyItemByQuery( map, requestFields, query,queryType);

            string iterationName;

            if (queryResult.Results.Any())
            {
                //var iter = queryResult.Results.Select(e => new Iteration(e)).Where(r => r.StartDate <= DateTime.Now).OrderByDescending(r => r.StartDate).First();
                var iter =
                    queryResult.Results.Select(
                        e =>
                            new
                            {
                                Name = e["Name"],
                                StartDate = e["StartDate"] == null ? DateTime.MinValue : DateTime.Parse(e["StartDate"]),
                                EndDate = e["EndDate"] == null ? DateTime.MaxValue : DateTime.Parse(e["EndDate"])
                            }).OrderByDescending(p=>p.StartDate).First();
                iterationName = iter.Name;

            }
            else
            {
                return null;
            }

            //User stories
            queryType = "hierarchicalrequirement";
            query = new Query("Iteration.Name", Query.Operator.Equals, iterationName);
            requestFields = new List<string>()
            {
                "Name",
                "ObjectID",
                "FormattedID",
                "Description",
                "Blocked",
                "BlockedReason",
                "Owner"
            };
            requestFields.Add(map.KanbanSortColumn);
            var result0 = GetRallyItemByQuery( map, requestFields, query, queryType);

            //Defects
            queryType = "defect";
            var result1 = GetRallyItemByQuery(map, requestFields, query, queryType);
            var result = result0.Results.ToList();
            result.AddRange(result1.Results);
            return result;
        }



    }
}
