﻿using System.ComponentModel.Composition;
using System.Windows;

namespace Panda.Client
{
    public class Launcher : Window
    {
        // todo: make it so escape minimizes
        // todo: close makes it minimize
        [Import]
        public SettingsService SettingsService { get; set; }
    }
}