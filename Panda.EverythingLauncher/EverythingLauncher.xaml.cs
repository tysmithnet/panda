using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
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

        private BehaviorSubject<string> TextChangedObservable { get; set; } = new BehaviorSubject<string>("");

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(SearchText.Text))
                TextChangedObservable.OnNext(SearchText.Text);
        }

        private void EverythingLauncher_OnActivated(object sender, EventArgs e)
        {                                                                            
            ViewModel = new EverythingLauncherViewModel(EverythingService, FileSystemContextMenuProviders, TextChangedObservable);
            DataContext = ViewModel;
        }
               
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.HandleSelectedResultsChanged(ResultsListBox.SelectedItems.Cast<EverythingResultViewModel>());
        }
    }
}