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
        public AppLauncher()
        {
            InitializeComponent();
        }
    }
}
