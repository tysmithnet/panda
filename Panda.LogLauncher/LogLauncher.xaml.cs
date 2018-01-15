using System.ComponentModel;
using System.ComponentModel.Composition;
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
    /// <seealso cref="Panda.Client.Launcher" />
    /// <seealso cref="Panda.Client.IRequiresSetup" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    [Export(typeof(Launcher))]
    [Export(typeof(IRequiresSetup))]
    public partial class LogLauncher : Launcher, IRequiresSetup
    {
        /// <summary>
        ///     The search text changed subject
        /// </summary>
        private readonly Subject<string> _searchTextChangedSubject = new Subject<string>();

        /// <summary>
        ///     The collection view source
        /// </summary>
        private ICollectionView _collectionViewSource;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogLauncher" /> class.
        /// </summary>
        public LogLauncher()
        {
            InitializeComponent();
            SearchTextBox.Focus();
        }

        /// <summary>
        ///     Gets or sets the log service.
        /// </summary>
        /// <value>The log service.</value>
        [Import]
        private ILogService LogService { get; set; }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public LogLauncherViewModel ViewModel { get; set; }

        /// <summary>
        ///     Performs any setup logic required
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task, that when complete, will indicate that the setup is complete</returns>
        public Task Setup(CancellationToken cancellationToken)
        {
            ViewModel =
                new LogLauncherViewModel(UiScheduler, LogService) // todo: handle large messages, color code level
                {
                    SearchTextChangedObs = _searchTextChangedSubject
                };
            DataContext = ViewModel;
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the SearchTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var newText = SearchTextBox.Text;
            _searchTextChangedSubject.OnNext(newText);
            _collectionViewSource.Refresh();
        }

        /// <summary>
        ///     Handles the OnLoaded event of the LogLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void LogLauncher_OnLoaded(object sender, RoutedEventArgs e)
        {
            _collectionViewSource = CollectionViewSource.GetDefaultView(LogMessagesDataGrid.ItemsSource);
            ViewModel.CollectionViewSource = _collectionViewSource;
        }
    }
}