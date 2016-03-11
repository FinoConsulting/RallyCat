using FluentData;
using RallyCat.Core.Configuration;
using RallyCat.Core.Interfaces;


namespace RallyCat.Core.DataAccess
{
    public class RallyGlobalConfigurationRepository : IRallyGlobalConfigurationRepository
    {
        private readonly IDbContext _DbContext;

        public RallyGlobalConfigurationRepository(IDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public Result<RallyGlobalConfiguration> GetItem()
        {
            var result = new Result<RallyGlobalConfiguration>();
            using (var context = _DbContext)
            {
                var item = context.Sql(@"GlobalVariables_FetchAll_AsPivot")
                    .CommandType(DbCommandTypes.StoredProcedure)
                    .QuerySingle<RallyGlobalConfiguration>();

                result.Object  = item;
                result.Success = item != null;
            }
            return result;
        }
    }
}