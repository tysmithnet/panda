using System.ComponentModel;
using System.Runtime.CompilerServices;
using Panda.Client.Properties;

namespace Panda.AppLauncher
{
    public sealed class AppViewModel : INotifyPropertyChanged
    {
        public string AppName { get; set; }
        public string ExecutableLocation { get; set; }
        public RegisteredApplication RegisteredApplication { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}