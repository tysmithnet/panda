using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
