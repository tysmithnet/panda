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
    [Export(typeof(Launcher))]
    public partial class WikipediaLauncher : Launcher
    {
        /// <summary>
        ///     The search text changed subject
        /// </summary>
        public Subject<string> SearchTextChangedSubject = new Subject<string>();

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Panda.Wikipedia.WikipediaLauncher" /> class.
        /// </summary>
        public WikipediaLauncher()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the wikipedia service.
        /// </summary>
        /// <value>
        ///     The wikipedia service.
        /// </value>
        [Import]
        public IWikipediaService WikipediaService { get; set; }

        /// <summary>
        ///     Gets or sets the item mouse double click subject.
        /// </summary>
        /// <value>
        ///     The item mouse double click subject.
        /// </value>
        public Subject<(WikipediaResultViewModel, MouseButtonEventArgs)> ItemMouseDoubleClickSubject { get; set; } =
            new Subject<(WikipediaResultViewModel, MouseButtonEventArgs)>();

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>
        ///     The view model.
        /// </value>
        public WikipediaLauncherViewModel ViewModel { get; set; }

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
                ItemMouseDoubleClickObs = ItemMouseDoubleClickSubject
            };
            DataContext = ViewModel;
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
    }
}