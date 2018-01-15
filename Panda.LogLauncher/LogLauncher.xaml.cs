﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Concurrency;
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
using Panda.Client;

namespace Panda.LogLauncher
{
    /// <summary>
    /// Interaction logic for LogLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    [Export(typeof(IRequiresSetup))]
    public partial class LogLauncher : Launcher, IRequiresSetup
    {
        [Import]
        internal ILogService LogService { get; set; }

        [Import]
        internal IScheduler UiScheduler { get; set; }

        public LogLauncher()
        {
            InitializeComponent();
        }
                                    
        public LogLauncherViewModel ViewModel { get; set; }
        public Task Setup(CancellationToken cancellationToken)
        {
            ViewModel = new LogLauncherViewModel(UiScheduler, LogService);
            DataContext = ViewModel;
            return Task.CompletedTask;
        }
    }
}
