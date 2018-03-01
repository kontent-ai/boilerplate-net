using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudBoilerplateNet.Services
{
    public interface IRelatedTypesResolver
    {
        IEnumerable<string> GetRelatedTypes(string typeCodeName);
    }
}
