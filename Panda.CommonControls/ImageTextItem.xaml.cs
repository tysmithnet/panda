using System;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Panda.CommonControls
{
    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ImageTextItem : UserControl
    {
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(ImageTextItem));

        public static readonly DependencyProperty SubHeaderTextProperty =
            DependencyProperty.Register("SubHeaderText", typeof(string), typeof(ImageTextItem));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageTextItem));

        public ImageTextItem()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        protected internal Subject<string> HeaderTextChangedSubject { get; set; } = new Subject<string>();
        protected internal Subject<string> SubHeaderChangedSubject { get; set; } = new Subject<string>();
        protected internal Subject<ImageSource> ImageSourceChangedSubject { get; set; } = new Subject<ImageSource>();

        protected internal Subject<MouseButtonEventArgs> PreviewMouseButtonUpSubject { get; set; } =
            new Subject<MouseButtonEventArgs>();

        public IObservable<MouseButtonEventArgs> PreviewMouseButtonUpObs => PreviewMouseButtonUpSubject;

        public string HeaderText
        {
            get => GetValue(HeaderTextProperty) as string;
            set => SetValue(HeaderTextProperty, value);
        }

        public string SubHeaderText
        {
            get => GetValue(SubHeaderTextProperty) as string;
            set => SetValue(SubHeaderTextProperty, value);
        }

        public ImageSource ImageSource
        {
            get => GetValue(ImageSourceProperty) as ImageSource;
            set => SetValue(ImageSourceProperty, value);
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            HeaderTextChangedSubject.OnNext(HeaderTextBox.Content as string);
        }

        private void SubHeaderText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SubHeaderChangedSubject.OnNext(SubHeaderTextBox.Content as string);
        }

        private void ImageTextItem_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }
    }
}