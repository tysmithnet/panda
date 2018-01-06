using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    /// <summary>
    ///     Service that will manage the interaction with es.exe
    /// </summary>
    [Export(typeof(IEverythingService))]
    [Export(typeof(IFileSystemSearch))]
    public sealed class EverythingService : IEverythingService, IFileSystemSearch
    {
        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        private ILog Log { get; } = LogManager.GetLogger<EverythingService>();

        /// <summary>
        ///     Gets or sets the settings service.
        /// </summary>
        /// <value>
        ///     The settings service.
        /// </value>
        [Import]
        internal ISettingsService SettingsService { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Searches the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        IObservable<EverythingResult> IEverythingService.Search(string query, CancellationToken cancellationToken)
        {
            return Search(query, cancellationToken).Select(line => new EverythingResult
            {
                FullPath = line
            });
        }

        /// <inheritdoc />
        /// <summary>
        ///     Searches for files using the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        IObservable<FileInfo> IFileSystemSearch.Search(string query, CancellationToken cancellationToken)
        {
            return Search(query, cancellationToken).Select(line => new FileInfo(line));
        }

        /// <summary>
        ///     Searches using the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Files and directories that match the query</returns>
        /// <exception cref="ConfigurationErrorsException"></exception>
        internal IObservable<string> Search(string query, CancellationToken cancellationToken)
        {
            var executablePath = SettingsService.Get<EverythingSettings>().Single().EsExePath;
            if (string.IsNullOrWhiteSpace(executablePath))
                throw new ConfigurationErrorsException($"es.exe is not set in everything launcher settings");
            var obs = Observable.Create<string>(async (observer, token) =>
            {
                await Task.Run(async () =>
                {
                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = executablePath,
                            Arguments = query,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    Log.Debug($"Started: {process.Id}");
                    string line;
                    while ((line = await process.StandardOutput.ReadLineAsync()) != null)
                    {
                        Log.Debug($"Process Line: {process.Id} - {line}");
                        observer.OnNext(line);

                        if (!cancellationToken.IsCancellationRequested && !token.IsCancellationRequested) continue;
                        Log.Debug($"Killing: {process.Id}");
                        process.Kill();
                        observer.OnCompleted();
                        return;
                    }
                    Log.Debug($"Finished: {process.Id}");
                    observer.OnCompleted();
                }, cancellationToken);
            });
            return obs;
        }
    }
}