using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Panda.Client;
using Point = System.Drawing.Point;

namespace Panda.EverythingLauncher
{
    /// <summary>
    ///     Interaction logic for EverythingLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public partial class EverythingLauncher : Launcher
    {
        public EverythingLauncher()
        {
            InitializeComponent();
        }

        [Import]
        public EverythingService EverythingService { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public EverythingLauncherViewModel ViewModel { get; set; }

        public IDisposable Subscription { get; set; }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
            Subscription?.Dispose();
            ViewModel.EverythingResults.Clear();                 
            Subscription = EverythingService.Search(ViewModel.SearchText, CancellationTokenSource.Token).Subscribe(
                result =>
                {
                    var resultVm = new EverythingResultViewModel
                    {
                        Name = result.FullPath
                    };
                    Application.Current.Dispatcher.Invoke(() => { ViewModel.EverythingResults.Add(resultVm); });
                });                        
        }

        private void EverythingLauncher_OnActivated(object sender, EventArgs e)
        {
            ViewModel = new EverythingLauncherViewModel();
            DataContext = ViewModel;
        }
    }
}