using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Panda.Client
{
    public sealed class LauncherViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        internal Launcher Instance { get; set; }
                 
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Select()
        {
        }
    }
}