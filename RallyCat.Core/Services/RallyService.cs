using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Authentication;
using Rally.RestApi;
using Rally.RestApi.Response;
using RallyCat.Core.Rally;
using RallyApi = Rally.RestApi;


namespace RallyCat.Core.Services
{
    public class RallyService
    {
        private readonly RallyApiConnectionPool _Pool;
        private readonly String                 _RallyToken;
        private readonly String                 _RallyUrl;

        public RallyService(RallyBackgroundDataService backgroundDataService)
        {
            _Pool       = new RallyApiConnectionPool();
            _RallyToken = backgroundDataService.RallyGlobalConfiguration.RallyToken;
            _RallyUrl   = backgroundDataService.RallyGlobalConfiguration.RallyUrl;
        }

        public QueryResult GetRallyItemById(RallySlackMapping map, String formattedId)
        {
            var queryType = "hierarchicalrequirement";
            if (formattedId.StartsWith("de", StringComparison.InvariantCultureIgnoreCase))
            {
                queryType = "defect";
            }

            var query         = new RallyApi.Query("FormattedID", RallyApi.Query.Operator.Equals, formattedId);
            var requestFields = new List<String> { "Name", "Description", "FormattedID" };
            return GetRallyItemByQuery(map, requestFields, query, queryType);
        }

        public QueryResult GetRallyItemByQuery(RallySlackMapping map, List<String> requestFields, RallyApi.Query query, String artifectName = "")
        {
            var api = _Pool.GetApi(_RallyToken, _RallyUrl);
            if (api == null) { throw new AuthenticationException("Cannot verify rally login"); }

            var request = String.IsNullOrEmpty(artifectName) 
                ? new RallyApi.Request() 
                : new RallyApi.Request(artifectName);

            request.Project   = "/project/" + map.ProjectId;
            request.Workspace = "/workspace/" + map.WorkspaceId;
            //request.Fetch   = new List<string>() { "Name", "Description", "FormattedID" };
            request.Fetch     = requestFields;
            request.Query     = query;
            var queryResult   = api.Query(request);

            return queryResult;
        }

        public Dictionary<string, List<KanbanItem>> GetOrderedColumns(RallySlackMapping map)
        {
            var api = _Pool.GetApi(_RallyToken, _RallyUrl);
            if (api == null)
            {
                throw new AuthenticationException("Cannot verify rally login");
            }
            var queryType           = "AttributeDefinition/46444394085/AllowedValues";
            var requestField        = new List<string>() { "StringValue", "ValueIndex" };
            Request request         = new Request(queryType);
            request.Fetch           = requestField;
            QueryResult queryResult = api.Query(request);


            if (queryResult.Results.Any())
            {
                var kanbanColumns = new Dictionary<string, List<KanbanItem>>();
                foreach (var item in queryResult.Results)
                {
                    var columnName = item["StringValue"];
                    if (item["StringValue"] == "")
                    {
                        columnName = "None";
                    }
                    if (!item["StringValue"].Contains("DE Approv"))
                    kanbanColumns.Add(columnName, null);
                }
                return kanbanColumns;
            }
            return null;
        }

        public List<dynamic> GetKanban(RallySlackMapping map)
        {
            // todo: review -- move this into a method call, then collate the result sets

            var queryType = "Iteration";
            var query = new RallyApi.Query(String.Format("(StartDate <= {0})", DateTime.Now.ToString("o")));

            // todo: review -- move this into a view model / DTO

            var requestFields = new List<String> { "Name", "StartDate", "Project", "EndDate" };
            var queryResult = GetRallyItemByQuery(map, requestFields, query, queryType);

            if (!queryResult.Results.Any()) { return null; }
            var iter = queryResult.Results.Select(result =>
                new
                {
                    Name      = result["Name"],
                    StartDate = result["StartDate"] == null ? DateTime.MinValue : DateTime.Parse(result["StartDate"]),
                    EndDate   = result["EndDate"]   == null ? DateTime.MaxValue : DateTime.Parse(result["EndDate"])
                }).OrderByDescending(p => p.StartDate).First();

            //User stories
            queryType     = "hierarchicalrequirement";
            query         = new RallyApi.Query("Iteration.Name", RallyApi.Query.Operator.Equals, iter.Name);
            requestFields = new List<String>
            {
                map.KanbanSortColumn,
                "Name",
                "ObjectID",
                "FormattedID",
                "Description",
                "Blocked",
                "BlockedReason",
                "Owner",
<<<<<<< HEAD
                map.KanbanSortColumn
            };
=======
                "DisplayColor"
            };
            // requestFields.Add(map.KanbanSortColumn);

            var result0 = GetRallyItemByQuery( map, requestFields, query, queryType);
>>>>>>> rallycat-slash-command

            var result0 = GetRallyItemByQuery(map, requestFields, query, queryType);

            //Defects
            queryType      = "defect";
            var result1    = GetRallyItemByQuery(map, requestFields, query, queryType);
            var resultList = result0.Results.ToList();

            resultList.AddRange(result1.Results);

            return resultList;
        }
    }
}