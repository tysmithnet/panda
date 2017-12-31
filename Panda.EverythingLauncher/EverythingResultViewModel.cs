using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    public sealed class EverythingResultViewModel : INotifyPropertyChanged
    {
        private ImageSource _icon;

        public EverythingResultViewModel(string fullName)
        {
            FullName = fullName;
            Name = Path.GetFileName(fullName);
            Directory = Path.GetDirectoryName(fullName);
        }

        public string FullName { get; set; }
        public string Directory { get; set; }
        public string Name { get; set; }

        public ImageSource Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Task LoadIcon()
        {
            return Task.Run(() =>
            {
                try
                {
                    Icon = IconHelper.IconFromFilePath(FullName);
                }
                catch (Exception)
                {
                    // todo: fallback icon
                }
            });
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}