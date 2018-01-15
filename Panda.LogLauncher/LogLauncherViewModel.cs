using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Panda.LogLauncher.Annotations;

namespace Panda.LogLauncher
{
    public class LogLauncherViewModel : INotifyPropertyChanged
    {
        private ILogService _logService;
        private IScheduler _uiScheduler;
        private IDisposable _logMessageSubscription;
        private IList<ILogMessage> _logMessages = new List<ILogMessage>();

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

                    });
            }
        }

        public LogLauncherViewModel(IScheduler uiScheduler, ILogService logService)
        {
            _uiScheduler = uiScheduler;
            _logService = logService;
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