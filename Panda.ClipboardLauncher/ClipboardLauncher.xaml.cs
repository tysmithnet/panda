using System.ComponentModel.Composition;
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
            ViewModel = new ClipboardLauncherViewModel(this, SettingsService);
            DataContext = ViewModel;
        }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>
        ///     The view model.
        /// </value>
        private ClipboardLauncherViewModel ViewModel { get; }
    }
}