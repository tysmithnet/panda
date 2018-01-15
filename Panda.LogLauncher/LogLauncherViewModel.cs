using System.ComponentModel;
using System.Runtime.CompilerServices;
using Panda.LogLauncher.Annotations;

namespace Panda.LogLauncher
{
    public class LogLauncherViewModel : INotifyPropertyChanged
    {
        public string Greeting { get; set; } = "hello world";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}