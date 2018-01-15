using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using Common.Logging;
using Panda.LogLauncher.Annotations;

namespace Panda.LogLauncher
{
    public class LogLauncherViewModel : INotifyPropertyChanged
    {
        private ILogService _logService;
        private IScheduler _uiScheduler;
        private IDisposable _logMessageSubscription;
        private IList<LogMessageViewModel> _logMessages = new List<LogMessageViewModel>();
        private string _searchText;
        private IObservable<string> _searchTextChangedObs;

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
                        _logMessages.Add(vm);
                        LogMessages.Add(vm);
                    });
            }
        }

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

        public LogLauncherViewModel(IScheduler uiScheduler, ILogService logService)
        {
            _uiScheduler = uiScheduler;
            LogService = logService;
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
                                                                        
        public ObservableCollection<LogMessageViewModel> LogMessages { get; set; } = new ObservableCollection<LogMessageViewModel>();

        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } = new ObservableCollection<FrameworkElement>();

        private IDisposable _searchTextChangedSubscription;
        private IObservable<(string, FilterEventArgs)> _filterObs;

        public IObservable<string> SearchTextChangedObs
        {
            get => _searchTextChangedObs;
            set
            {
                _searchTextChangedSubscription?.Dispose();
                _searchTextChangedObs = value;
                _searchTextChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(_uiScheduler)
                    .Subscribe(s =>
                    {              
                    });
            }
        }

        private IDisposable _filterSubscription;
        private ICollectionView _collectionViewSource;

        public IObservable<(string, FilterEventArgs)> FilterObs
        {
            get => _filterObs;
            set
            {
                _filterSubscription?.Dispose();
                _filterObs = value;
                _filterSubscription = value.SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(_uiScheduler)
                    .Subscribe(tuple =>
                    {
                        // todo: parallelize
                        var searchText = tuple.Item1;
                        var vm = tuple.Item2.Item as LogMessageViewModel;
                        tuple.Item2.Accepted = vm.Message.Contains(searchText);
                    });

            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}