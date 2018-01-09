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
    public class WikipediaLauncherViewModel : INotifyPropertyChanged
    {
        private IObservable<string> _searchTextChangedObs;
        public event PropertyChangedEventHandler PropertyChanged;
        
        private IDisposable _searchTextChangedSubscription;

        public WikipediaLauncherViewModel(WikipediaService wikipediaService)
        {
            WikipediaService = wikipediaService;
        }

        private IDisposable _searchResultsSubscription;
        private IObservable<(WikipediaResultViewModel, MouseButtonEventArgs)> _itemMouseDoubleClickObs;

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
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            WikipediaResultViewModels.Clear();
                        });
                        _searchResultsSubscription?.Dispose();
                        _searchResultsSubscription = WikipediaService.AutoComplete(text, CancellationToken.None).Subscribe(
                            result =>
                            {
                                var vm = new WikipediaResultViewModel
                                {
                                    Title = result.Title,
                                    Description = result.Description,
                                    Url = new Uri(result.Url)
                                };
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    WikipediaResultViewModels.Add(vm);  
                                });
                            });
                    });
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<WikipediaResultViewModel> WikipediaResultViewModels { get; set; } = new ObservableCollection<WikipediaResultViewModel>();
        public WikipediaService WikipediaService { get; private set; }

        private IDisposable _itemMouseDoubleClickSubscription;
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

                    Process.Start(vm.Url.AbsoluteUri); //todo: application service should be able to open browsers for provided urls
                });
            }
        }
    }
}