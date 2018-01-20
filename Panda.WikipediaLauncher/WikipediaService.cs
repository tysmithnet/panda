using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using Common.Logging;
using Newtonsoft.Json.Linq;

namespace Panda.WikipediaLauncher
{
    /// <inheritdoc />
    /// <summary>
    ///     Implementation for Wikipedia service
    /// </summary>
    /// <seealso cref="T:Panda.WikipediaLauncher.IWikipediaService" />
    [Export(typeof(IWikipediaService))]
    internal sealed class WikipediaService : IWikipediaService
    {
        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>The log.</value>
        private ILog Log { get; } = LogManager.GetLogger<WikipediaService>();

        /// <inheritdoc />
        /// <summary>
        ///     Get a small list of wikipedia potential matches
        /// </summary>
        /// <param name="search">The search.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An observable stream of wikipedia results matching the provided query</returns>
        public IObservable<WikipediaResult> AutoComplete(string search, CancellationToken cancellationToken)
        {
            var obs = Observable.Create<WikipediaResult>(async (observer, token) =>
            {
                // todo: abstract http (wikipedia likes when you use a friendly user agent string)
                var request = (HttpWebRequest) WebRequest.Create(
                    $"https://en.wikipedia.org/w/api.php?action=opensearch&search={search}&limit=10&namespace=0&format=json");
                request.Credentials = CredentialCache.DefaultCredentials;
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;
                HttpWebResponse response;

                try
                {
                    response = (HttpWebResponse) await request.GetResponseAsync();
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                    Log.Error($"Error getting response stream: {e.Message}");
                    return;
                }

                var resStream = response.GetResponseStream();

                if (resStream == null)
                {
                    observer.OnError(new InvalidOperationException(
                        $"Wikipedia response stream was null"));
                    Log.Error($"Wikipedia response stream was null");
                }

                var reader = new StreamReader(resStream);

                string responseFromServer;
                try
                {
                    responseFromServer = await reader.ReadToEndAsync();
                    Log.Debug($"Response from server: {responseFromServer}");
                }
                catch (Exception e)
                {
                    Log.Error($"Could not get response from server: {e.Message}");
                    observer.OnError(e);
                    return;
                }

                JArray root;
                try
                {
                    root = JArray.Parse(responseFromServer);
                }
                catch (Exception e)
                {
                    Log.Error($"Error parsing response from server: {e.Message}");
                    observer.OnError(e);
                    return;
                }

                var toBeZipped = root.OfType<JArray>();
                var arrays = toBeZipped.Select(j => j.Values<string>().ToArray()).ToArray();
                var n = arrays[0]
                    .Length; // api seems to return 3 empty arrays so its fine to always check for the first
                for (var i = 0; i < n; i++)
                    try
                    {
                        var title = arrays[0][i];
                        var description = arrays[1][i];
                        var url = arrays[2][i];
                        var newResult = new WikipediaResult
                        {
                            Title = title,
                            Description = description,
                            Url = url
                        };
                        observer.OnNext(newResult);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error creating WikipediaResult: {e.Message}");
                        observer.OnError(e);
                    }
                observer.OnCompleted();
            });

            return obs;
        }
    }
}