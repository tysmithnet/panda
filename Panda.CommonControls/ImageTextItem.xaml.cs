using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Panda.CommonControls.Annotations;

namespace Panda.CommonControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ImageTextItem : UserControl
    {                                     
        protected internal Subject<string> HeaderTextChangedSubject { get; set; } = new Subject<string>();
        protected internal Subject<string> SubHeaderChangedSubject { get; set; } = new Subject<string>();
        protected internal Subject<ImageSource> ImageSourceChangedSubject { get; set; } = new Subject<ImageSource>();

        public string HeaderText
        {
            get => base.GetValue(HeaderTextProperty) as string;
            set => base.SetValue(HeaderTextProperty, value);
        }
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(ImageTextItem));

        public string SubHeaderText
        {
            get => base.GetValue(SubHeaderTextProperty) as string;
            set => base.SetValue(SubHeaderTextProperty, value);
        }
        public static readonly DependencyProperty SubHeaderTextProperty = DependencyProperty.Register("SubHeaderText", typeof(string), typeof(ImageTextItem));

        public string ImageSource
        {
            get => base.GetValue(ImageSourceProperty) as string;
            set => base.SetValue(ImageSourceProperty, value);
        }
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(string), typeof(ImageTextItem));

        public ImageTextItem()
        {
            InitializeComponent();
            ViewModel = new ImageTextItemViewModel
            {
                   ImageSourceChangedObs = ImageSourceChangedSubject,
                   HeaderTextChangedObs = HeaderTextChangedSubject,
                   SubHeaderTextChangedObs = SubHeaderChangedSubject
            };
            DataContext = ViewModel;
        }

        public ImageTextItemViewModel ViewModel { get; set; }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            HeaderTextChangedSubject.OnNext(HeaderTextBox.Text);
        }

        private void SubHeaderText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SubHeaderChangedSubject.OnNext(SubHeaderTextBox.Text);
        }
    }

    public class ImageTextItemViewModel : INotifyPropertyChanged
    {
        private IObservable<ImageSource> _imageSourceChangedObs;

        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        public string HeaderText
        {
            get => _headerText;
            set
            {
                _headerText = value; 
                OnPropertyChanged();
            }
        }

        public string SubHeaderText
        {
            get => _subHeaderText;
            set
            {
                _subHeaderText = value; 
                OnPropertyChanged();
            }
        }

        private IDisposable _imageSourceChangedSubscription;
        private IObservable<string> _headerTextChangedObs;

        public IObservable<ImageSource> ImageSourceChangedObs
        {
            get => _imageSourceChangedObs;
            set
            {
                _imageSourceChangedSubscription?.Dispose();
                _imageSourceChangedObs = value;
                _imageSourceChangedSubscription = value.Subscribe(imageSource =>
                {
                    ImageSource = imageSource;
                });

            }
        }

        private IDisposable _headerTextChangedSubscription;
        private IObservable<string> _subHeaderTextChangedObs;

        public IObservable<string> HeaderTextChangedObs
        {
            get => _headerTextChangedObs;
            set
            {
                _headerTextChangedSubscription?.Dispose();
                _headerTextChangedObs = value;
                _headerTextChangedSubscription = value.Subscribe(s =>
                {
                    HeaderText = s;
                });
            }
        }

        private IDisposable _subHeaderTextChangedSubscription;
        private ImageSource _imageSource;
        private string _headerText;
        private string _subHeaderText;

        public IObservable<string> SubHeaderTextChangedObs
        {
            get => _subHeaderTextChangedObs;
            set
            {
                _subHeaderTextChangedSubscription?.Dispose();
                _subHeaderTextChangedObs = value;
                _subHeaderTextChangedSubscription = value.Subscribe(s =>
                {
                    SubHeaderText = s;
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
