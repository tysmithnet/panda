using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Common.Logging;
using Panda.LogLauncher.Annotations;

namespace Panda.LogLauncher
{
    /// <summary>
    ///     Class LogLauncherViewModel.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class LogLauncherViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The UI scheduler
        /// </summary>
        private readonly IScheduler _uiScheduler;

        /// <summary>
        ///     The collection view source
        /// </summary>
        private ICollectionView _collectionViewSource;

        /// <summary>
        ///     The log message subscription
        /// </summary>
        private IDisposable _logMessageSubscription;

        /// <summary>
        ///     The log service
        /// </summary>
        private ILogService _logService;

        /// <summary>
        ///     The search text
        /// </summary>
        private string _searchText;

        /// <summary>
        ///     The search text changed obs
        /// </summary>
        private IObservable<string> _searchTextChangedObs;

        /// <summary>
        ///     The search text changed subscription
        /// </summary>
        private IDisposable _searchTextChangedSubscription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogLauncherViewModel" /> class.
        /// </summary>
        /// <param name="uiScheduler">The UI scheduler.</param>
        /// <param name="logService">The log service.</param>
        public LogLauncherViewModel(IScheduler uiScheduler, ILogService logService)
        {
            _uiScheduler = uiScheduler;
            LogService = logService;
        }

        /// <summary>
        ///     Gets or sets the log service.
        /// </summary>
        /// <value>The log service.</value>
        private ILogService LogService
        {
            get => _logService;
            set
            {
                _logMessageSubscription?.Dispose();
                _logService = value;
                _logMessageSubscription = value.LogMessages
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(_uiScheduler)
                    .Subscribe(message =>
                    {
                        var vm = new LogMessageViewModel
                        {
                            LogName = message.LogName,
                            LogLevel = TranslateLogLevel(message.Level),
                            Message = message.Message,
                            Exception = TranslateException(message.Exception),
                            LogTime = message.LogTime
                        };
                        LogMessages.Add(vm);
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the search text.
        /// </summary>
        /// <value>The search text.</value>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the log messages.
        /// </summary>
        /// <value>The log messages.</value>
        public ObservableCollection<LogMessageViewModel> LogMessages { get; set; } =
            new ObservableCollection<LogMessageViewModel>();

        /// <summary>
        ///     Gets or sets the context menu items.
        /// </summary>
        /// <value>The context menu items.</value>
        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

        /// <summary>
        ///     Gets or sets the collection view source.
        /// </summary>
        /// <value>The collection view source.</value>
        public ICollectionView CollectionViewSource
        {
            get => _collectionViewSource;
            set
            {
                _collectionViewSource = value;
                value.Filter = o =>
                {
                    var vm = o as LogMessageViewModel;
                    return vm?.Message?.Contains(SearchText ?? "") ?? false;
                };
            }
        }

        /// <summary>
        ///     Gets or sets the refresh data grid action.
        /// </summary>
        /// <value>The refresh data grid action.</value>
        public Action RefreshDataGridAction { get; set; }

        /// <summary>
        ///     Gets or sets the search text changed obs.
        /// </summary>
        /// <value>The search text changed obs.</value>
        public IObservable<string> SearchTextChangedObs
        {
            get => _searchTextChangedObs;
            set
            {
                _searchTextChangedSubscription?.Dispose();
                _searchTextChangedObs = value;
                _searchTextChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(s => { });
            }
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Translates the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>System.String.</returns>
        private string TranslateException(Exception exception)
        {
            if (exception == null)
                return "";
            return $"{exception.GetType().FullName} - {exception.Message}{Environment.NewLine}{exception.StackTrace}";
        }

        /// <summary>
        ///     Translates the log level.
        /// </summary>
        /// <param name="messageLevel">The message level.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="ArgumentOutOfRangeException">messageLevel</exception>
        private string TranslateLogLevel(LogLevel messageLevel)
        {
            switch (messageLevel)
            {
                case LogLevel.All:
                    return "All";
                case LogLevel.Debug:
                    return "Debug";
                case LogLevel.Error:
                    return "Error";
                case LogLevel.Fatal:
                    return "Fatal";
                case LogLevel.Info:
                    return "Info";
                case LogLevel.Off:
                    return "Off";
                case LogLevel.Trace:
                    return "Trace";
                case LogLevel.Warn:
                    return "Warn";
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(messageLevel)} was not a valid log level");
            }
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}