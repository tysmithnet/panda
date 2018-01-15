using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Panda.Client;

namespace Panda.LogLauncher
{
    /// <summary>
    ///     Interaction logic for LogLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    [Export(typeof(IRequiresSetup))]
    public partial class LogLauncher : Launcher, IRequiresSetup
    {
        private readonly Subject<string> _searchTextChangedSubject = new Subject<string>();

        private ICollectionView _collectionViewSource;

        public LogLauncher()
        {
            InitializeComponent();
            SearchTextBox.Focus();
        }

        [Import]
        internal ILogService LogService { get; set; }

        [Import]
        internal IScheduler UiScheduler { get; set; }

        public LogLauncherViewModel ViewModel { get; set; }

        public Task Setup(CancellationToken cancellationToken)
        {
            ViewModel = new LogLauncherViewModel(UiScheduler, LogService)   // todo: handle large messages, color code level
            {
                SearchTextChangedObs = _searchTextChangedSubject,
            };
            DataContext = ViewModel;
            return Task.CompletedTask;
        }

        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var newText = SearchTextBox.Text;
            _searchTextChangedSubject.OnNext(newText);
            _collectionViewSource.Refresh();
        }

        private void LogLauncher_OnLoaded(object sender, RoutedEventArgs e)
        {
            _collectionViewSource = CollectionViewSource.GetDefaultView(LogMessagesDataGrid.ItemsSource);
            ViewModel.CollectionViewSource = _collectionViewSource;
        }
    }
}