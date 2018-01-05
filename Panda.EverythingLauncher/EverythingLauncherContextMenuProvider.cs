using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Common.Logging;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    [Export(typeof(IFileSystemContextMenuProvider))]
    public sealed class EverythingLauncherContextMenuProvider : IFileSystemContextMenuProvider
    {
        [Import]
        internal IEventHub EventHub { get; set; }

        private ILog Log { get; } = LogManager.GetLogger<EverythingLauncherContextMenuProvider>();

        public bool CanHandle(IEnumerable<FileInfo> items)
        {
            return items.Any();
        }

        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<FileInfo> items)
        {
            var menuItem = new MenuItem();
            var arr = items.ToArray();
            menuItem.Header = $"Delete {arr.Length} Items";
            
            menuItem.Click += (sender, args) =>
            {
                foreach (var fileInfo in arr)
                {
                    try
                    {
                        File.Delete(fileInfo.FullName);
                        EventHub.Broadcast(new FileDeletedEvent(fileInfo.FullName));
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Could not delete {fileInfo.FullName} - {e.Message}");
                    }
                }
            };      

            return new[] {menuItem};
        }
    }
}
