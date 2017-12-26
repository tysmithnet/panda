using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Panda.Client;

namespace Panda.AppLauncher
{
    [Export(typeof(IFileSystemContextMenuProvider))]
    public class AppLauncherContextMenuProvider : IFileSystemContextMenuProvider
    {
        [Import]
        public SettingsService SettingsService { get; set; }

        public bool CanHandle(IEnumerable<FileInfo> fileInfos)
        {
            return fileInfos.Any();
        }

        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<FileInfo> fileInfos)
        {
            var appLauncherSettings = SettingsService.Get<AppLauncherSettings>().Single();
            var items = new List<MenuItem>();
            var menuItem = new MenuItem
            {
                Header = "Add to Applications"
            };
            menuItem.Click += (sender, args) =>
            {
                SettingsService.Save(CancellationToken.None);
            };
            foreach (var fileInfo in fileInfos)
            {              
                menuItem.Click += (sender, args) =>
                {
                    var registerdApp = new RegisteredApplication
                    {
                        FullPath = fileInfo.FullName,
                        DisplayName = fileInfo.Name
                    };
                    appLauncherSettings.RegisteredApplications.Add(registerdApp);
                };
                items.Add(menuItem);
            }                                            
            return items;
        }
    }
}
