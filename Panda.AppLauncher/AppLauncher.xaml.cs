using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using Panda.Client;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Panda.AppLauncher
{
    /// <summary>
    /// Interaction logic for AppLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public partial class AppLauncher : Launcher
    {
        [Import]
        public RegisteredApplicationRepository RegisteredApplicationRepository { get; set; }

        [ImportMany]
        public IRegisteredApplicationContextMenuProvider[] RegisteredApplicationContextMenuProviders { get; set; }

        public AppLauncher()
        {
            InitializeComponent();
        }

        private void AppLauncher_OnActivated(object sender, EventArgs e)
        {
            ViewModel = new AppLauncherViewModel(RegisteredApplicationRepository, RegisteredApplicationContextMenuProviders);
            DataContext = ViewModel;
        }

        public AppLauncherViewModel ViewModel { get; set; }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = RegisteredApplications.SelectedItems.Cast<AppViewModel>();
            ViewModel.HandleSelectedItemsChanged(selectedItems);
        }   
    }
}
