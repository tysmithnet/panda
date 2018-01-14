using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
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

        /// <summary>
        ///     The clipboard item mouse up subject
        /// </summary>
        private readonly Subject<(ClipboardItemViewModel, MouseButtonEventArgs)> _clipboardItemMouseUpSubject =
            new Subject<(ClipboardItemViewModel, MouseButtonEventArgs)>();

        private readonly Subject<ClipboardItemViewModel> _clipboardItemSelected = new Subject<ClipboardItemViewModel>();

        [Import] private IClipboardService _clipboardService;

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
            ViewModel = new ClipboardLauncherViewModel(UiScheduler, _clipboardService, SettingsService)
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

        /// <summary>
        ///     Handles the OnMouseUp event of the ClipboardItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void ClipboardItem_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ImageTextItem;
            var vm = item.DataContext as ClipboardItemViewModel;
            _clipboardItemMouseUpSubject.OnNext((vm, e));
        }
    }
}