using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Panda.Client;
using Panda.Wikipedia.Annotations;
using System.Reactive.Subjects;
using Panda.CommonControls;

namespace Panda.Wikipedia
{
    // todo: weather
    // todo: traffic
    // todo: msdn documentation
    /// <summary>
    /// Interaction logic for WikipediaLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public partial class WikipediaLauncher : Launcher
    {             
        [Import]
        public WikipediaService WikipediaService { get; set; }

        public Subject<string> SearchTextChangedSubject = new Subject<string>();
        public Subject<(WikipediaResultViewModel, MouseButtonEventArgs)> ItemMouseDoubleClickSubject { get; set; }= new Subject<(WikipediaResultViewModel, MouseButtonEventArgs)>();
        public WikipediaLauncher()
        {
            InitializeComponent();   
        }

        public WikipediaLauncherViewModel ViewModel { get; set; }

        private void SearchText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = SearchText.Text;                                           
            SearchTextChangedSubject.OnNext(text);
        }

        private void ListItem_OnKeyUp(object sender, KeyEventArgs e)
        {
            ;
        }

        private void WikipediaLauncher_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = new WikipediaLauncherViewModel(WikipediaService)
            {
                SearchTextChangedObs = SearchTextChangedSubject,
                ItemMouseDoubleClickObs = ItemMouseDoubleClickSubject
            };
            DataContext = ViewModel;
        }

        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e)
        {
            //mouse double click
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var imageTextItem = sender as ImageTextItem;
            var vm = imageTextItem?.DataContext as WikipediaResultViewModel;
            ItemMouseDoubleClickSubject.OnNext((vm, e));
        }
    }

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

    public class WikipediaResultViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Uri Url { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WikipediaResult
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }

    [Export]
    public class WikipediaService
    {                                                      
        public IObservable<WikipediaResult> AutoComplete(string search, CancellationToken cancellationToken)
        {
            var obs = Observable.Create<WikipediaResult>(async (observer, token) =>
            {
                // todo: abstract http (wikipedia likes when you use a friendly user agent string)
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://en.wikipedia.org/w/api.php?action=opensearch&search={search}&limit=10&namespace=0&format=json");
                request.Credentials = CredentialCache.DefaultCredentials;                                                                                                            
                ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

                var response = (HttpWebResponse) await request.GetResponseAsync();
                var resStream = response.GetResponseStream();
                var reader = new StreamReader(resStream ?? throw new InvalidOperationException($"Wikipedia response stream was null"));
                var responseFromServer = await reader.ReadToEndAsync();

                var root = JArray.Parse(responseFromServer);
                var toBeZipped = root.OfType<JArray>();
                var arrays = toBeZipped.Select(j => j.Values<string>().ToArray()).ToArray();
                // todo: assert all are the same size, but we can probably trust wikipedia
                int n = arrays[0].Length; // api seems to return 3 empty arrays so its fine to always check for the first
                for (int i = 0; i < n; i++)
                {
                    var title = arrays[0][i];
                    var description = arrays[1][i];
                    var url = arrays[2][i];
                    var newResult = new WikipediaResult // todo: error checking
                    {
                        Title = title,
                        Description = description,
                        Url = url
                    };
                    observer.OnNext(newResult);
                }
                observer.OnCompleted();
            });

            return obs;               
        }
    }
}
