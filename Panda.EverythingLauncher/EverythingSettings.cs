using System.ComponentModel.Composition;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    /// <summary>
    ///     Settings for the everything launcher
    /// </summary>
    /// <seealso cref="Panda.Client.IPluginSettings" />
    /// <inheritdoc />
    /// <seealso cref="T:Panda.Client.IPluginSettings" />
    [Export(typeof(IPluginSettings))]
    internal class EverythingSettings : IPluginSettings
    {
        /// <summary>
        ///     Gets or sets the es.exe executable path.
        /// </summary>
        /// <value>The es executable path.</value>
        public string EsExePath { get; set; }

        /// <summary>
        ///     Gets or sets the maximum number of results from a query
        /// </summary>
        /// <value>The search limit.</value>
        public int SearchLimit { get; set; } = 100;
    }
}