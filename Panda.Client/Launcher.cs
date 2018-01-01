using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;

namespace Panda.Client
{
    public class Launcher : Window
    {
        // todo: make it so escape minimizes
        // todo: close makes it minimize
        [Import]
        protected internal ISettingsService SettingsService { get; set; }

        protected Launcher()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}