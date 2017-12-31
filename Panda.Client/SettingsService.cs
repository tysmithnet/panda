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
    [Export(typeof(SettingsService))]
    public class SettingsService
    {
        protected ILog Log = LogManager.GetLogger<SettingsService>();

        [ImportMany]
        protected internal IPluginSettings[] AllPluginSettings { get; set; }

        public IEnumerable<TSettings> Get<TSettings>() where TSettings : IPluginSettings
        {
            return AllPluginSettings.OfType<TSettings>();
        }

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

        public void Save()
        {
            foreach (var settings in AllPluginSettings)
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText($"{settings.GetType().FullName}.config.json", json);
            }
        }
    }
}