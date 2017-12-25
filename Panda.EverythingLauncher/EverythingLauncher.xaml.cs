using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    /// <summary>
    ///     Interaction logic for EverythingLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public partial class EverythingLauncher : Launcher
    {
        public EverythingLauncher()
        {
            InitializeComponent();
        }

        public EverythingLauncherViewModel ViewModel { get; set; }

        [Import]
        public EverythingService EverythingService { get; set; }

        [ImportMany]
        public IFileSystemContextMenuProvider[] FileSystemContextMenuProviders { get; set; }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.HandleSearchTextChanged(e);
        }

        private void EverythingLauncher_OnActivated(object sender, EventArgs e)
        {
            ViewModel = new EverythingLauncherViewModel(EverythingService, FileSystemContextMenuProviders);
            DataContext = ViewModel;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.HandleSelectedResultsChanged(ResultsListBox.SelectedItems.Cast<EverythingResultViewModel>());
        }
    }
}