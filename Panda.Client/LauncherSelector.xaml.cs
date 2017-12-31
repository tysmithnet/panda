using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Panda.Client
{
    /// <summary>
    /// Interaction logic for LauncherSelector.xaml
    /// </summary>
    [Export(typeof(LauncherSelector))]
    public partial class LauncherSelector : Window
    {      
        [Import]
        protected internal LauncherRepository LauncherRepository { get; set; }

        [Import]
        protected internal KeyboardMouseHookService KeyboardMouseHookService { get; set; }

        private LauncherSelectorViewModel ViewModel { get; set; }
        private BehaviorSubject<string> TextChangedObservable { get; set; } = new BehaviorSubject<string>("");

        public LauncherSelector()
        {   
            InitializeComponent(); 
        }
                   
        private void LauncherSelector_OnActivated(object sender, EventArgs e)
        {
            SearchText.Focus();
            KeyboardMouseHookService.KeyDownObservable.Throttle(TimeSpan.FromMilliseconds(100)) // todo: setting
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(args =>
            {
                if (args.Alt && args.KeyCode.HasFlag(Keys.Space))
                {
                    
                    WindowState = WindowState.Normal;
                    //Topmost = true;
                    Show();
                    Activate();
                    Focus();
                    SearchText.Focus();
                    args.Handled = true;
                }
            });
            ViewModel = new LauncherSelectorViewModel(LauncherRepository, KeyboardMouseHookService, TextChangedObservable);
            DataContext = ViewModel;
        }
              
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.Handle(e); // todo: rename
        }                                       

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {   
            TextChangedObservable.OnNext(SearchText.Text);
        }

        private void LauncherSelector_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {   
            if (e.Key == Key.Escape)
            {
                Hide();
            }

            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
               ViewModel.Submit(); 
            }
        }
    }
}
