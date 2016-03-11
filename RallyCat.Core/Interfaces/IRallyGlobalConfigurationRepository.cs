using RallyCat.Core.Configuration;
using RallyCat.Core.DataAccess;


namespace RallyCat.Core.Interfaces
{
    public interface IRallyGlobalConfigurationRepository
    {
        Result<RallyGlobalConfiguration> GetItem();
    }
}