using System;
using System.Collections.Generic;
using System.Linq;
using FluentData;
using RallyCat.Core.Interfaces;
using RallyCat.Core.Rally;

namespace RallyCat.Core.DataAccess
{
    public class RallySlackMappingRepository : IRallySlackMappingRepository
    {
        public IDbContext DbContext;

        public RallySlackMappingRepository(IDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public Result<List<RallySlackMapping>> GetAll()
        {
            var result = new Result<List<RallySlackMapping>>();
            using (var context = DbContext)
            {
                var query = @"select * from RallyConfigurations";
                var item =
                    context.Sql(query).QueryMany((RallySlackMapping o, IDataReader r) =>
                    {
                        o.Id               =  (Int32)   r["Id"              ];
                        o.TeamName         =  (String)  r["TeamName"        ];
                        o.ProjectId        =  (Int64)   r["ProjectId"       ];
                        o.WorkspaceId      =  (Int64)   r["WorkspaceId"     ];
                        o.KanbanSortColumn =  (String)  r["KanbanSortColumn"];
                        o.EnableKanban     =  (Boolean) r["EnableKanban"    ];
                        o.Channels         = ((String)  r["Channels"        ]).Split(',').ToList();
                    });

                if (item == null)
                {
                    result.Success = false;
                    result.Object  = null;
                    return result;
                }
                result.Object  = item;
                result.Success = true;
            }

            return result;
        }
    }
}