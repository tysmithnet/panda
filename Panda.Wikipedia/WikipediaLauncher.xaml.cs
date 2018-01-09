using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
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
using Panda.Client;
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
           
        private void WikipediaLauncher_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = new WikipediaLauncherViewModel(WikipediaService)
            {
                SearchTextChangedObs = SearchTextChangedSubject,
                ItemMouseDoubleClickObs = ItemMouseDoubleClickSubject
            };
            DataContext = ViewModel;
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var imageTextItem = sender as ImageTextItem;
            var vm = imageTextItem?.DataContext as WikipediaResultViewModel;
            ItemMouseDoubleClickSubject.OnNext((vm, e));
        }
    }
}
