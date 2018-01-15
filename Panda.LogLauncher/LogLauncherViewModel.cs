using System;
using System.Collections.Generic;
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
        private ILogService _logService;
        private IScheduler _uiScheduler;
        private IDisposable _logMessageSubscription;
        private IList<LogMessageViewModel> _logMessages = new List<LogMessageViewModel>();

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
                                                              
        public string SearchText { get; set; }

        public ObservableCollection<LogMessageViewModel> LogMessages { get; set; } = new ObservableCollection<LogMessageViewModel>();

        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } = new ObservableCollection<FrameworkElement>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}