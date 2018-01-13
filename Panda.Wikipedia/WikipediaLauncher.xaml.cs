using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Panda.Client;
using Panda.CommonControls;

namespace Panda.Wikipedia
{
    /// <summary>
    ///     Interaction logic for WikipediaLauncher.xaml
    /// </summary>
    /// <seealso cref="Panda.Client.Launcher" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    [Export(typeof(Launcher))]
    public sealed partial class WikipediaLauncher : Launcher
    {
        /// <summary>
        ///     The list box key up subject
        /// </summary>
        private readonly Subject<KeyEventArgs> _listBoxKeyUpSubject = new Subject<KeyEventArgs>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Panda.Wikipedia.WikipediaLauncher" /> class.
        /// </summary>
        /// <inheritdoc />
        public WikipediaLauncher()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     The search text changed subject
        /// </summary>
        /// <value>The search text changed subject.</value>
        private Subject<string> SearchTextChangedSubject { get; } = new Subject<string>();

        /// <summary>
        ///     Gets or sets the wikipedia service.
        /// </summary>
        /// <value>The wikipedia service.</value>
        [Import]
        private IWikipediaService WikipediaService { get; set; }

        /// <summary>
        ///     Gets or sets the item mouse double click subject.
        /// </summary>
        /// <value>The item mouse double click subject.</value>
        private Subject<(WikipediaResultViewModel, MouseButtonEventArgs)> ItemMouseDoubleClickSubject { get; } =
            new Subject<(WikipediaResultViewModel, MouseButtonEventArgs)>();

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        private WikipediaLauncherViewModel ViewModel { get; set; }

        /// <summary>
        ///     Gets the selection changed subject.
        /// </summary>
        /// <value>The selection changed subject.</value>
        private Subject<(WikipediaResultViewModel, SelectionChangedEventArgs)> SelectionChangedSubject { get; } =
            new Subject<(WikipediaResultViewModel, SelectionChangedEventArgs)>();

        /// <summary>
        ///     Handles the OnTextChanged event of the SearchText control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void SearchText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = SearchText.Text;
            SearchTextChangedSubject.OnNext(text);
        }

        /// <summary>
        ///     Handles the OnLoaded event of the WikipediaLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void WikipediaLauncher_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = new WikipediaLauncherViewModel(UiScheduler, WikipediaService)
            {
                SearchTextChangedObs = SearchTextChangedSubject,
                ItemMouseDoubleClickObs = ItemMouseDoubleClickSubject,
                SelectionChangedObs = SelectionChangedSubject,
                ListBoxKeyUpObs = _listBoxKeyUpSubject
            };
            DataContext = ViewModel;
            SearchText.Focus();
        }

        /// <summary>
        ///     Handles the OnMouseDoubleClick event of the Control control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var imageTextItem = sender as ImageTextItem;
            var vm = imageTextItem?.DataContext as WikipediaResultViewModel;
            ItemMouseDoubleClickSubject.OnNext((vm, e));
        }

        /// <summary>
        ///     Selectors the on selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="System.Windows.Controls.SelectionChangedEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = e.AddedItems[0] as WikipediaResultViewModel;
            SelectionChangedSubject.OnNext((vm, e));
        }

        /// <summary>
        ///     UIs the element on key up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs" /> instance containing the event data.</param>
        private void UIElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            _listBoxKeyUpSubject.OnNext(e);
        }
    }
}