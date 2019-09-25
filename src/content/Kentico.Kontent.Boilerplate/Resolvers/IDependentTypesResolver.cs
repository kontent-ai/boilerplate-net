using System.Collections.Generic;

namespace Kentico.Kontent.Boilerplate.Resolvers
{
    /// <summary>
    /// Resolves a list of dependencies that are known/discoverable at build time.
    /// </summary>
    public interface IDependentTypesResolver
    {
        /// <summary>
        /// Gets identifiers of types (formats) of data that depend on the type described by <paramref name="typeCodeName"/>.
        /// </summary>
        /// <param name="typeCodeName">Name of the type that others depend on.</param>
        /// <returns>Identifiers of dependent types.</returns>
        IEnumerable<string> GetDependentTypeNames(string typeCodeName);
    }
}
