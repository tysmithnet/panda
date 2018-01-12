using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Panda.Wikipedia.Annotations;

namespace Panda.Wikipedia
{
    /// <summary>
    /// View Model for wikipedia launcher
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class WikipediaLauncherViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The item mouse double click observable
        /// </summary>
        private IObservable<(WikipediaResultViewModel, MouseButtonEventArgs)> _itemMouseDoubleClickObs;

        /// <summary>
        /// The item mouse double click subscription
        /// </summary>
        private IDisposable _itemMouseDoubleClickSubscription;

        /// <summary>
        /// The search results subscription
        /// </summary>
        private IDisposable _searchResultsSubscription;

        /// <summary>
        /// The search text changed observable
        /// </summary>
        private IObservable<string> _searchTextChangedObs;

        /// <summary>
        /// The search text changed subscription
        /// </summary>
        private IDisposable _searchTextChangedSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="WikipediaLauncherViewModel" /> class.
        /// </summary>
        /// <param name="uiScheduler">The UI scheduler.</param>
        /// <param name="wikipediaService">The wikipedia service.</param>
        /// <exception cref="NullReferenceException">wikipediaService</exception>
        public WikipediaLauncherViewModel(IScheduler uiScheduler, IWikipediaService wikipediaService)
        {
            UiScheduler = uiScheduler;
            _wikipediaService = wikipediaService ?? throw new NullReferenceException(nameof(wikipediaService));
        }

        /// <summary>
        /// Gets or sets the UI scheduler.
        /// </summary>
        /// <value>The UI scheduler.</value>
        public IScheduler UiScheduler { get; set; }


        /// <summary>
        /// Gets or sets the search text changed obs.
        /// </summary>
        /// <value>The search text changed obs.</value>
        internal IObservable<string> SearchTextChangedObs
        {
            get => _searchTextChangedObs;
            set
            {
                _searchTextChangedSubscription?.Dispose();
                _searchTextChangedObs = value;
                _searchTextChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Throttle(TimeSpan.FromMilliseconds(333))
                    .Subscribe(text =>
                    {
                        Application.Current.Dispatcher.Invoke(() => { WikipediaResultViewModels.Clear(); });
                        _searchResultsSubscription?.Dispose();
                        _searchResultsSubscription = _wikipediaService
                            .AutoComplete(text, CancellationToken.None)
                            .SubscribeOn(TaskPoolScheduler.Default)
                            .ObserveOn(UiScheduler)
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
        /// Gets or sets the wikipedia result view models.
        /// </summary>
        /// <value>The wikipedia result view models.</value>
        public ObservableCollection<WikipediaResultViewModel> WikipediaResultViewModels { get; set; } =
            new ObservableCollection<WikipediaResultViewModel>();

        /// <summary>
        /// Gets the wikipedia service.
        /// </summary>
        /// <value>
        /// The wikipedia service.
        /// </value>
        private IWikipediaService _wikipediaService;

        /// <summary>
        /// Gets or sets the item mouse double click obs.
        /// </summary>
        /// <value>The item mouse double click obs.</value>
        internal IObservable<(WikipediaResultViewModel, MouseButtonEventArgs)> ItemMouseDoubleClickObs
        {
            get => _itemMouseDoubleClickObs;
            set
            {
                _itemMouseDoubleClickSubscription?.Dispose();
                _itemMouseDoubleClickObs = value;
                _itemMouseDoubleClickSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(tuple =>
                {
                    var vm = tuple.Item1;
                    var args = tuple.Item2;

                    Process.Start(vm.Url
                        .AbsoluteUri); //todo: application service should be able to open browsers for provided urls
                });
            }
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}