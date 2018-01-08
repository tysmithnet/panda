using System.Collections;
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

        /// <summary>
        ///     The is editable property
        /// </summary>
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(ImageTextItem));

        /// <summary>
        ///     The menu items property
        /// </summary>
        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register("MenuItems", typeof(IEnumerable), typeof(ImageTextItem));

        /// <summary>
        ///     The stop editing command
        /// </summary>
        public static RoutedCommand StopEditingCommand = new RoutedCommand("StopEditing", typeof(ImageTextItem));


        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageTextItem" /> class.
        /// </summary>
        public ImageTextItem()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

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

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is editable.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditable
        {
            get => (bool) GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }

        /// <summary>
        ///     Gets or sets the menu items.
        /// </summary>
        /// <value>
        ///     The menu items.
        /// </value>
        public IEnumerable MenuItems
        {
            get => GetValue(MenuItemsProperty) as IEnumerable;
            set => SetValue(MenuItemsProperty, value);
        }

        /// <summary>
        ///     Handles the OnExecuted event of the CommandBinding control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs" /> instance containing the event data.</param>
        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            IsEditable = false;
        }
    }
}