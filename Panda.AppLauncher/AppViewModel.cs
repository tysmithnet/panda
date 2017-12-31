using System.ComponentModel;
using System.Runtime.CompilerServices;
using Panda.Client.Properties;

namespace Panda.AppLauncher
{
    public class AppViewModel : INotifyPropertyChanged
    {
        protected internal string AppName { get; set; }
        protected internal string ExecutableLocation { get; set; }
        protected internal RegisteredApplication RegisteredApplication { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}