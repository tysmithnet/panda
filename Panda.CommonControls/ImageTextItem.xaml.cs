﻿using System;
using System.Globalization;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Panda.CommonControls
{
    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ImageTextItem : UserControl
    {        
        /// <summary>
        ///     The header text property
        /// </summary>
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(ImageTextItem));

        /// <summary>
        ///     The sub header text property
        /// </summary>
        public static readonly DependencyProperty SubHeaderTextProperty =
            DependencyProperty.Register("SubHeaderText", typeof(string), typeof(ImageTextItem));

        /// <summary>
        ///     The image source property
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageTextItem));

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(ImageTextItem));

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageTextItem" /> class.
        /// </summary>
        public ImageTextItem()
        {   
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        /// <summary>
        ///     Gets or sets the header text changed subject.
        /// </summary>
        /// <value>
        ///     The header text changed subject.
        /// </value>
        protected internal Subject<string> HeaderTextChangedSubject { get; set; } = new Subject<string>();

        /// <summary>
        ///     Gets or sets the sub header changed subject.
        /// </summary>
        /// <value>
        ///     The sub header changed subject.
        /// </value>
        protected internal Subject<string> SubHeaderChangedSubject { get; set; } = new Subject<string>();

        /// <summary>
        ///     Gets or sets the image source changed subject.
        /// </summary>
        /// <value>
        ///     The image source changed subject.
        /// </value>
        protected internal Subject<ImageSource> ImageSourceChangedSubject { get; set; } = new Subject<ImageSource>();

        /// <summary>
        ///     Gets or sets the preview mouse button up subject.
        /// </summary>
        /// <value>
        ///     The preview mouse button up subject.
        /// </value>
        protected internal Subject<MouseButtonEventArgs> PreviewMouseButtonUpSubject { get; set; } =
            new Subject<MouseButtonEventArgs>();

        /// <summary>
        ///     Gets the preview mouse button up obs.
        /// </summary>
        /// <value>
        ///     The preview mouse button up obs.
        /// </value>
        public IObservable<MouseButtonEventArgs> PreviewMouseButtonUpObs => PreviewMouseButtonUpSubject;

        /// <summary>
        ///     Gets or sets the header text.
        /// </summary>
        /// <value>
        ///     The header text.
        /// </value>
        public string HeaderText
        {
            get => GetValue(HeaderTextProperty) as string;
            set => SetValue(HeaderTextProperty, value);
        }

        /// <summary>
        ///     Gets or sets the sub header text.
        /// </summary>
        /// <value>
        ///     The sub header text.
        /// </value>
        public string SubHeaderText
        {
            get => GetValue(SubHeaderTextProperty) as string;
            set => SetValue(SubHeaderTextProperty, value);
        }

        /// <summary>
        ///     Gets or sets the image source.
        /// </summary>
        /// <value>
        ///     The image source.
        /// </value>
        public ImageSource ImageSource
        {
            get => GetValue(ImageSourceProperty) as ImageSource;
            set => SetValue(ImageSourceProperty, value);
        }

        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }

        /// <summary>
        ///     Handles the OnPreviewMouseUp event of the ImageTextItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void ImageTextItem_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // todo: needed?
        }        
    }

    public class VisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = System.Convert.ToBoolean(value ?? false);
            var invertResult = System.Convert.ToBoolean(parameter ?? false);
            isVisible = invertResult ? !isVisible : isVisible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}