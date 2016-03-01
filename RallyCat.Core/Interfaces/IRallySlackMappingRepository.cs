using System.Collections.Generic;
using RallyCat.Core.DataAccess;
using RallyCat.Core.Rally;

namespace RallyCat.Core.Interfaces
{
    public interface IRallySlackMappingRepository
    {
        Result<List<RallySlackMapping>> GetAll();
    }
}