using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using Panda.Client;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     Interaction logic for AppLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public partial class AppLauncher : Launcher
    {
        public AppLauncher()
        {
            InitializeComponent();
        }

        [Import]
        protected internal RegisteredApplicationRepository RegisteredApplicationRepository { get; set; }

        [ImportMany]
        protected internal IRegisteredApplicationContextMenuProvider[] RegisteredApplicationContextMenuProviders { get; set; }

        protected internal AppLauncherViewModel ViewModel { get; set; }

        private void AppLauncher_OnActivated(object sender, EventArgs e)
        {
            ViewModel = new AppLauncherViewModel(RegisteredApplicationRepository,
                RegisteredApplicationContextMenuProviders);
            DataContext = ViewModel;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = RegisteredApplications.SelectedItems.Cast<AppViewModel>();
            ViewModel.HandleSelectedItemsChanged(selectedItems);
        }
    }
}