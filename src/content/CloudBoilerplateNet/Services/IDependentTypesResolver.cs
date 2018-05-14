using System.Collections.Generic;

namespace CloudBoilerplateNet.Services
{
    public interface IDependentTypesResolver
    {
        IEnumerable<string> GetDependentTypeNames(string typeCodeName);
    }
}
