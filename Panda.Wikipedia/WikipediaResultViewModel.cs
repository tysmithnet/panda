using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Panda.Wikipedia.Annotations;

namespace Panda.Wikipedia
{
    public class WikipediaResultViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Uri Url { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}