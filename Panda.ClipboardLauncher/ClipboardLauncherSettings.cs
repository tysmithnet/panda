using System.ComponentModel.Composition;
using Panda.Client;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    /// Class ClipboardLauncherSettings.
    /// </summary>
    /// <seealso cref="IPluginSettings" />
    [Export(typeof(IPluginSettings))]
    internal class ClipboardLauncherSettings : IPluginSettings
    {
        /// <summary>
        /// Gets or sets the size of the clipboard history.
        /// </summary>
        /// <value>The size of the clipboard history.</value>
        public uint ClipboardHistorySize { get; set; } = 50;
    }
}