using System.ComponentModel.Composition;
using System.Windows;
using Panda.Client;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public sealed partial class ClipboardLauncher : Launcher
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClipboardLauncher" /> class.
        /// </summary>
        public ClipboardLauncher()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>
        ///     The view model.
        /// </value>
        private ClipboardLauncherViewModel ViewModel { get; set; }

        /// <summary>
        /// Handles the OnLoaded event of the ClipboardLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ClipboardLauncher_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = new ClipboardLauncherViewModel(this, SettingsService);
            DataContext = ViewModel;
        }
    }
}