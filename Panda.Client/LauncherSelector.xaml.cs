using System;
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

namespace Panda.Client
{
    /// <summary>
    /// Interaction logic for LauncherSelector.xaml
    /// </summary>
    [Export(typeof(LauncherSelector))]
    public partial class LauncherSelector : Window
    {      
        [ImportMany]
        public IEnumerable<Launcher> Launchers { get; set; }

        public LauncherSelectorViewModel ViewModel { get; set; }                                    

        public LauncherSelector()
        {   
            InitializeComponent();
            
        }
                   
        private void LauncherSelector_OnActivated(object sender, EventArgs e)
        {
            ViewModel = new LauncherSelectorViewModel(Launchers);
            DataContext = ViewModel;
        }
          

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var launcherVm = (LauncherViewModel) e.AddedItems[0];
            Hide();
            launcherVm.Instance.Show();
        }
    }
}
