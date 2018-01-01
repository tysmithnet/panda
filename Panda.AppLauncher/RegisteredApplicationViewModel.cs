using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Panda.Client;
using Panda.Client.Properties;

namespace Panda.AppLauncher
{
    public sealed class RegisteredApplicationViewModel : INotifyPropertyChanged
    {
        private ImageSource _imageSource;
        public string AppName { get; set; }
        public string ExecutableLocation { get; set; }
        public RegisteredApplication RegisteredApplication { get; set; }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        public Task LoadIcon()
        {
            return Task.Run(() =>
            {
                ImageSource = IconHelper.IconFromFilePath(ExecutableLocation);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}