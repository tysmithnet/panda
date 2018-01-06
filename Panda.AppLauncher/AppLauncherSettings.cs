﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using Panda.Client;

namespace Panda.AppLauncher
{
    /// <inheritdoc />
    /// <summary>
    ///     Settings for the Registered Application domain
    /// </summary>
    /// <seealso cref="T:Panda.Client.IPluginSettings" />
    [Export(typeof(IPluginSettings))]
    public class AppLauncherSettings : IPluginSettings
    {
        /// <summary>
        ///     Gets or sets the registered applications.
        /// </summary>
        /// <value>
        ///     The registered applications.
        /// </value>
        public IList<RegisteredApplication> RegisteredApplications { get; set; } = new List<RegisteredApplication>();

        public uint SearchDepth { get; set; } = 2;

        public IList<string> SearchPaths = new List<string>()
        {
            
        };
    }
}