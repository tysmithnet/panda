using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Panda.Wikipedia.Annotations;

namespace Panda.Wikipedia
{
    /// <summary>
    ///     View Model for wikipedia launcher
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class WikipediaLauncherViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The item mouse double click observable
        /// </summary>
        private IObservable<(WikipediaResultViewModel, MouseButtonEventArgs)> _itemMouseDoubleClickObs;

        /// <summary>
        ///     The item mouse double click subscription
        /// </summary>
        private IDisposable _itemMouseDoubleClickSubscription;

        /// <summary>
        ///     The search results subscription
        /// </summary>
        private IDisposable _searchResultsSubscription;

        /// <summary>
        ///     The search text changed observable
        /// </summary>
        private IObservable<string> _searchTextChangedObs;

        /// <summary>
        ///     The search text changed subscription
        /// </summary>
        private IDisposable _searchTextChangedSubscription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WikipediaLauncherViewModel" /> class.
        /// </summary>
        /// <param name="wikipediaService">The wikipedia service.</param>
        public WikipediaLauncherViewModel(WikipediaService wikipediaService)
        {
            WikipediaService = wikipediaService;
        }

        /// <summary>
        ///     Gets or sets the search text changed obs.
        /// </summary>
        /// <value>
        ///     The search text changed obs.
        /// </value>
        internal IObservable<string> SearchTextChangedObs
        {
            get => _searchTextChangedObs;
            set
            {
                _searchTextChangedSubscription?.Dispose();
                _searchTextChangedObs = value;
                _searchTextChangedSubscription = value
                    .Throttle(TimeSpan.FromMilliseconds(333))
                    .Subscribe(text =>
                    {
                        Application.Current.Dispatcher.Invoke(() => { WikipediaResultViewModels.Clear(); });
                        _searchResultsSubscription?.Dispose();
                        _searchResultsSubscription = WikipediaService.AutoComplete(text, CancellationToken.None)
                            .Subscribe(
                                result =>
                                {
                                    var vm = new WikipediaResultViewModel
                                    {
                                        Title = result.Title,
                                        Description = result.Description,
                                        Url = new Uri(result.Url)
                                    };
                                    Application.Current.Dispatcher.Invoke(() => { WikipediaResultViewModels.Add(vm); });
                                });
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the wikipedia result view models.
        /// </summary>
        /// <value>
        ///     The wikipedia result view models.
        /// </value>
        public ObservableCollection<WikipediaResultViewModel> WikipediaResultViewModels { get; set; } =
            new ObservableCollection<WikipediaResultViewModel>();

        /// <summary>
        ///     Gets the wikipedia service.
        /// </summary>
        /// <value>
        ///     The wikipedia service.
        /// </value>
        public WikipediaService WikipediaService { get; }

        /// <summary>
        ///     Gets or sets the item mouse double click obs.
        /// </summary>
        /// <value>
        ///     The item mouse double click obs.
        /// </value>
        public IObservable<(WikipediaResultViewModel, MouseButtonEventArgs)> ItemMouseDoubleClickObs
        {
            get => _itemMouseDoubleClickObs;
            set
            {
                _itemMouseDoubleClickSubscription?.Dispose();
                _itemMouseDoubleClickObs = value;
                _itemMouseDoubleClickSubscription = value.Subscribe(tuple =>
                {
                    var vm = tuple.Item1;
                    var args = tuple.Item2;

                    Process.Start(vm.Url
                        .AbsoluteUri); //todo: application service should be able to open browsers for provided urls
                });
            }
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}