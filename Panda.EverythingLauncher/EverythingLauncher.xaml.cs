using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
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
        private BehaviorSubject<IEnumerable<EverythingResultViewModel>> SelectedItemsChangedObservable { get; set; } = new BehaviorSubject<IEnumerable<EverythingResultViewModel>>(null);
        private BehaviorSubject<MouseButtonEventArgs> PreviewMouseRightButtonDownObservable { get; set; } = new BehaviorSubject<MouseButtonEventArgs>(null);

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {                                                   
            TextChangedObservable.OnNext(SearchText.Text);
        }

        private void EverythingLauncher_OnActivated(object sender, EventArgs e)
        {
            SearchText.Focus();
            ViewModel = new EverythingLauncherViewModel(EverythingService, FileSystemContextMenuProviders, TextChangedObservable, SelectedItemsChangedObservable, PreviewMouseRightButtonDownObservable);
            DataContext = ViewModel;
        }
               
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var results = ResultsListBox.SelectedItems.Cast<EverythingResultViewModel>().ToList();
            SelectedItemsChangedObservable.OnNext(results);
        }

        private void ResultsListBox_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseRightButtonDownObservable.OnNext(e);
        }
    }
}