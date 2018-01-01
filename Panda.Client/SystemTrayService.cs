using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Panda.Client
{
    [Export(typeof(ISystemService))]
    public sealed class SystemTrayService : ISystemService
    {
        internal NotifyIcon Icon { get; set; }

        public Task Setup(CancellationToken token)
        {             
            using (var stream = Application.GetResourceStream(new Uri(@"pack://application:,,,/"
                                                                      + Assembly.GetExecutingAssembly().GetName().Name
                                                                      + ";component/"
                                                                      + "quicklogo.ico", UriKind.Absolute)).Stream)
            {
                Icon = new NotifyIcon
                {
                    Icon = new Icon(stream),
                    Visible = true,
                    ContextMenu = new ContextMenu()
                };
                Application.Current.Exit += (sender, args) =>
                {
                    Icon.Dispose();
                };
                var exitMenuItem = new MenuItem();
                exitMenuItem.Text = "Exit";
                exitMenuItem.Click += (sender, args) => Application.Current.Shutdown(0);
                Icon.ContextMenu.MenuItems.Add(exitMenuItem);
                return Task.CompletedTask;
            }                                
        }
    }
}