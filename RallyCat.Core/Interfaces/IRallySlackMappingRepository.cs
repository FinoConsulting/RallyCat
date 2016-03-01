using System.Collections.Generic;
using RallyCat.Core.DataAccess;
using RallyCat.Core.Rally;

namespace RallyCat.Core.Interfaces
{
    public interface IRallySlackMappingRepository
    {
        RallySlackMapping GetOne();
    
        Result<List<RallySlackMapping>> GetAll();
    }
}