using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Panda.LogLauncher.Annotations;

namespace Panda.LogLauncher
{
    public class LogMessageViewModel : INotifyPropertyChanged
    {
        public DateTime LogTime { get; set; }
        public string LogName { get; set; }
        public string LogLevel { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}