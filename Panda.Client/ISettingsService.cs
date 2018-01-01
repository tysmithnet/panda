using System.Collections.Generic;

namespace Panda.Client
{
    /// <summary>
    ///     Service that will manage the settings exposed by plugins
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        ///     Gets all settings of a certain type
        /// </summary>
        /// <typeparam name="TSettings">The type of the settings.</typeparam>
        /// <returns>All registered settings that match the provided type</returns>
        IEnumerable<TSettings> Get<TSettings>() where TSettings : IPluginSettings;

        /// <summary>
        ///     Saves the current setting values to disk
        /// </summary>
        void Save();
    }
}