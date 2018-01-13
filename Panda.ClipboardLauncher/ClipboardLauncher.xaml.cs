using System;
using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Panda.Client;
using Panda.CommonControls;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    /// <seealso cref="Panda.Client.Launcher" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    [Export(typeof(Launcher))]
    public sealed partial class ClipboardLauncher : Launcher
    {
        /// <summary>
        ///     The search text changed
        /// </summary>
        private readonly Subject<string> _searchTextChangedSubject = new Subject<string>();

        private Subject<string> _clipboardItemSelected = new Subject<string>();

        

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClipboardLauncher" /> class.
        /// </summary>
        public ClipboardLauncher()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        private ClipboardLauncherViewModel ViewModel { get; set; }

        /// <summary>
        ///     Handles the OnLoaded event of the ClipboardLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void ClipboardLauncher_OnLoaded(object sender, RoutedEventArgs e)
        {
            SearchText.Focus();
            ViewModel = new ClipboardLauncherViewModel(this, UiScheduler, SettingsService)
            {
                SearchTextChangedObs = _searchTextChangedSubject,
                ClipboardItemSelected = _clipboardItemSelected,
                ClipboardItemMouseUpObs = _clipboardItemMouseUpSubject 
            };
            DataContext = ViewModel;
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the SearchText control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void SearchText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTextChangedSubject.OnNext(SearchText.Text);
        }

        private Subject<(string, MouseButtonEventArgs)> _clipboardItemMouseUpSubject = new Subject<(string, MouseButtonEventArgs)>();

        private void ClipboardItem_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ImageTextItem;
            _clipboardItemMouseUpSubject.OnNext((item?.HeaderText, e));
        }
    }
}