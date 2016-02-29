using System.Collections.Generic;
using System.Linq;
using FluentData;
using RallyCat.Core.Interfaces;
using RallyCat.Core.Rally;

namespace RallyCat.Core.DataAccess
{
    public class RallySlackMappingRepository : IRallySlackMappingRepository
    {
        public IDbContext _dbContext;

        public RallySlackMappingRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public RallySlackMapping GetOne()
        {
            // todo: Quinn, 16.02.29.10.30 -- US1234
            //  this will be implemented in ...


            throw new System.NotImplementedException();
        }

        public Result<List<RallySlackMapping>> GetAll()
        {
            var result = new Result<List<RallySlackMapping>>();
            using (IDbContext context = _dbContext)
            {
                List<RallySlackMapping> item =
                    context.Sql(@"select * from RallyConfigurations").QueryMany((RallySlackMapping o, IDataReader r) =>
                    {
                        o.Id = (int) r["Id"];
                        o.TeamName = (string) r["TeamName"];
                        o.ProjectId = (long) r["ProjectId"];
                        o.WorkspaceId = (long) r["WorkspaceId"];
                        o.KanbanSortColumn = (string) r["KanbanSortColumn"];
                        o.EnableKanban = (bool) r["EnableKanban"];
                        o.Channels = ((string) r["Channels"]).Split(',').ToList();
                    });

                if (item == null)
                {
                    result.Success = false;
                    result.Object = null;
                    return result;
                }
                result.Object = item;
                result.Success = true;
            }
            return result;
        }
    }
}