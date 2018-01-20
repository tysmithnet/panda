using Newtonsoft.Json;

namespace Panda.Client
{
    /// <summary>
    ///     Overrides for types that should or should not be used when performing dependency injection
    /// </summary>
    internal class Bindings
    {
        /// <summary>
        ///     The black listed assemblies
        /// </summary>
        [JsonProperty("blacklisted_assemblies")] internal string[] BlackListedAssemblies = new string[0];

        /// <summary>
        ///     The black listed types
        /// </summary>
        [JsonProperty("blacklisted_types")] internal string[] BlackListedTypes = new string[0];
    }
}