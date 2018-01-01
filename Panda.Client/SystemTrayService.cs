using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Panda.Client
{
    /// <inheritdoc />
    /// <summary>
    ///     Service that will manage the system tray domain
    /// </summary>
    /// <seealso cref="T:Panda.Client.ISystemService" />
    [Export(typeof(ISystemService))]
    public sealed class SystemTrayService : ISystemService
    {
        /// <summary>
        ///     Gets or sets the icon for the system tray
        /// </summary>
        /// <value>
        ///     The icon.
        /// </value>
        internal NotifyIcon Icon { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Performs any required setup
        /// </summary>
        /// <param name="token">The cancellation token</param>
        /// <returns>
        ///     A task, that when complete, will indicate the completion of the setup process
        /// </returns>
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
                Application.Current.Exit += (sender, args) => { Icon.Dispose(); };
                var exitMenuItem = new MenuItem();
                exitMenuItem.Text = "Exit";
                exitMenuItem.Click += (sender, args) => Application.Current.Shutdown(0);
                Icon.ContextMenu.MenuItems.Add(exitMenuItem);
                return Task.CompletedTask;
            }
        }
    }
}