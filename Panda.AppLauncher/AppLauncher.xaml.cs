using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Panda.Client;

namespace Panda.AppLauncher
{
    /// <summary>
    /// Interaction logic for AppLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public partial class AppLauncher
    {
        [Import]
        public AppLauncherRepository AppLauncherRepository { get; set; }
                                      
        public AppLauncher()
        {
            InitializeComponent();
        }

        private void AppLauncher_OnActivated(object sender, EventArgs e)
        {
            ViewModel = new AppLauncherViewModel(AppLauncherRepository);
        }

        public AppLauncherViewModel ViewModel { get; set; }
    }
}
