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
        private readonly IDbContext _DbContext;

        public RallySlackMappingRepository(IDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public Result<List<RallySlackMapping>> GetAll()
        {
            var result = new Result<List<RallySlackMapping>>();
            using (var context = _DbContext)
            {
                var query = @"select * from RallyConfigurations";
                var item = context.Sql(query).QueryMany((RallySlackMapping o, IDataReader r) =>
                    {
                        o.Id               =  (Int32)   r["Id"              ];
                        o.TeamName         =  (String)  r["TeamName"        ];
                        o.ProjectId        =  (Int64)   r["ProjectId"       ];
                        o.WorkspaceId      =  (Int64)   r["WorkspaceId"     ];
                        o.KanbanSortColumn =  (String)  r["KanbanSortColumn"];
                        o.EnableKanban     =  (Boolean) r["EnableKanban"    ];
                        o.Channels         = ((String)  r["Channels"        ]).Split(',').ToList();
                    });

                result.Object  = item;
                result.Success = item != null;
            }

            return result;
        }
    }
}