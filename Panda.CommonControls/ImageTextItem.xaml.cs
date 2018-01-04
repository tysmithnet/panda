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
        protected internal Subject<MouseButtonEventArgs> PreviewMouseButtonUpSubject { get; set; } = new Subject<MouseButtonEventArgs>();
                                                   
        public IObservable<MouseButtonEventArgs> PreviewMouseButtonUpObs => PreviewMouseButtonUpSubject;

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

        public ImageSource ImageSource
        {
            get => base.GetValue(ImageSourceProperty) as ImageSource;
            set => base.SetValue(ImageSourceProperty, value);
        }
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageTextItem));

        public ImageTextItem()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
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
