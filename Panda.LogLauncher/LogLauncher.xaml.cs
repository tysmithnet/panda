using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Panda.Client;

namespace Panda.LogLauncher
{
    /// <summary>
    /// Interaction logic for LogLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    [Export(typeof(IRequiresSetup))]
    public partial class LogLauncher : Launcher, IRequiresSetup
    {
        [Import]
        internal ILogService LogService { get; set; }

        [Import]
        internal IScheduler UiScheduler { get; set; }

        public LogLauncher()
        {
            InitializeComponent();
        }
                                    
        public LogLauncherViewModel ViewModel { get; set; }
        public Task Setup(CancellationToken cancellationToken)
        {
            
            ViewModel = new LogLauncherViewModel(UiScheduler, LogService)
            {
                SearchTextChangedObs = _searchTextChangedSubject,
                FilterObs = _logMessagesViewSourceFilterSubject
            };
            DataContext = ViewModel;
            return Task.CompletedTask;
        }


        private Subject<string> _searchTextChangedSubject = new Subject<string>();
        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            string newText = SearchTextBox.Text;
            _searchTextChangedSubject.OnNext(newText);
            CollectionViewSource.GetDefaultView(LogMessagesDataGrid.ItemsSource).Refresh();
        }


        private Subject<(string, FilterEventArgs)> _logMessagesViewSourceFilterSubject = new Subject<(string, FilterEventArgs)>();
        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            string searchText = SearchTextBox.Text;
            _logMessagesViewSourceFilterSubject.OnNext((searchText, e));
        }

        private void LogLauncher_OnLoaded(object sender, RoutedEventArgs e)
        {                                                                                                   
            var collectionViewSource = CollectionViewSource.GetDefaultView(LogMessagesDataGrid.ItemsSource);
            ViewModel.CollectionViewSource = collectionViewSource;
        }
    }
}
