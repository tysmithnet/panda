﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panda.Client
{
    [Export]
    public class LauncherRepository
    {
        [ImportMany]
        protected internal Launcher[] Launchers { get; set; }

        public IEnumerable<Launcher> Search(string text)
        {
            return Launchers.Where(launcher => launcher.GetType().FullName.ToLower().Contains(text));
        }

        public IEnumerable<Launcher> Get()
        {
            return Launchers;
        }
    }
}