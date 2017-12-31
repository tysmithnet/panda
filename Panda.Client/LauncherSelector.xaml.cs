using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
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
            KeyboardMouseHookService.KeyDownObservable.Subscribe(args =>
            {
                if (args.Alt && args.KeyCode.HasFlag(Keys.Space))
                {

                    WindowState = WindowState.Normal;
                    Activate();
                    Topmost = true;
                    args.Handled = true;
                }
            });
            ViewModel = new LauncherSelectorViewModel(LauncherRepository, KeyboardMouseHookService, TextChangedObservable);
            DataContext = ViewModel;
        }
          

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var added = e.AddedItems.Cast<LauncherViewModel>();
            var launcherViewModels = added as LauncherViewModel[] ?? added.ToArray();
            if (launcherViewModels.Any())
            {
                var first = launcherViewModels.First();
                Active?.Hide();
                Active = first.Instance;
                Active.Show();
            }
        }

        public Launcher Active { get; set; }

        private void LauncherSelector_OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //Hide();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {   
            TextChangedObservable.OnNext(SearchText.Text);
        }
    }
}
