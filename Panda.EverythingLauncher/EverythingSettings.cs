using System.ComponentModel.Composition;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    /// <inheritdoc />
    /// <summary>
    ///     Settings for the everything launcher
    /// </summary>
    /// <seealso cref="T:Panda.Client.IPluginSettings" />
    [Export(typeof(IPluginSettings))]
    internal class EverythingSettings : IPluginSettings
    {
        /// <summary>
        ///     Gets or sets the es.exe executable path.
        /// </summary>
        /// <value>
        ///     The es executable path.
        /// </value>
        public string EsExePath { get; set; }
    }
}