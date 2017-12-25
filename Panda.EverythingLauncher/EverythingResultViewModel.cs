using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Panda.EverythingLauncher
{
    public class EverythingResultViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}