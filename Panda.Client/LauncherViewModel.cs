using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Panda.Client
{
    public class LauncherViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Launcher Instance { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Select()
        {
            
        }
    }
}