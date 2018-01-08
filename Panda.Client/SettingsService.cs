using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Newtonsoft.Json;

namespace Panda.Client
{
    /// <summary>
    ///     Service that will manage the settings domain
    /// </summary>
    /// <seealso cref="Panda.Client.ISettingsService" />
    /// <seealso cref="Panda.Client.ISystemService" />
    [Export(typeof(ISettingsService))]
    [Export(typeof(ISystemService))]
    public sealed class SettingsService : ISettingsService, ISystemService
    {
        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        private ILog Log { get; } = LogManager.GetLogger<SettingsService>();

        /// <summary>
        ///     Gets or sets all plugin settings.
        /// </summary>
        /// <value>
        ///     All plugin settings.
        /// </value>
        [ImportMany]
        internal IPluginSettings[] AllPluginSettings { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets all settings of a certain type
        /// </summary>
        /// <typeparam name="TSettings">The type of the settings.</typeparam>
        /// <returns>
        ///     All registered settings that match the provided type
        /// </returns>
        public IEnumerable<TSettings> Get<TSettings>() where TSettings : IPluginSettings
        {
            return AllPluginSettings.OfType<TSettings>();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Saves the current setting values to disk
        /// </summary>
        public void Save()
        {
            foreach (var settings in AllPluginSettings)
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText($"{settings.GetType().FullName}.config.json", json); // todo: error checking
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Setups the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task Setup(CancellationToken cancellationToken)
        {
            foreach (var settings in AllPluginSettings)
            {
                var fileName = $"{settings.GetType().FullName}.config.json";
                try
                {
                    var json = File.ReadAllText(fileName);
                    var revivedSettings = JsonConvert.DeserializeObject(json, settings.GetType());
                    foreach (var propertyInfo in revivedSettings.GetType().GetProperties())
                    {
                        var savedValue = propertyInfo.GetValue(revivedSettings);
                        propertyInfo.SetValue(settings, savedValue);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(
                        $"Unable to revive settings from {fileName} because {e.Message}, falling back to defaults");
                    try
                    {
                        File.WriteAllText(fileName, JsonConvert.SerializeObject(settings, Formatting.Indented));
                    }
                    catch (Exception e2)
                    {
                        Log.Fatal($"Could create new settings file for {settings.GetType()} because {e2.Message}");
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}