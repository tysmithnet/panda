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
    public class LogLauncherViewModel : INotifyPropertyChanged
    {
        private ICollectionView _collectionViewSource;
        private IDisposable _logMessageSubscription;
        private ILogService _logService;
        private string _searchText;
        private readonly IScheduler _uiScheduler;
        private IObservable<string> _searchTextChangedObs;

        public LogLauncherViewModel(IScheduler uiScheduler, ILogService logService)
        {
            _uiScheduler = uiScheduler;
            LogService = logService;
        }

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

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LogMessageViewModel> LogMessages { get; set; } =
            new ObservableCollection<LogMessageViewModel>();

        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

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

        public Action RefreshDataGridAction { get; set; }

        private IDisposable _searchTextChangedSubscription;
        public IObservable<string> SearchTextChangedObs
        {
            get => _searchTextChangedObs;
            set
            {
                _searchTextChangedSubscription?.Dispose();
                _searchTextChangedObs = value;
                _searchTextChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(s =>
                    {                                      
                    });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string TranslateException(Exception exception)
        {
            if (exception == null)
                return "";
            return $"{exception.GetType().FullName} - {exception.Message}{Environment.NewLine}{exception.StackTrace}";
        }

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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}