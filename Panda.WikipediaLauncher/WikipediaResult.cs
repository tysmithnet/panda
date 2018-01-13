namespace Panda.WikipediaLauncher
{
    /// <summary>
    ///     Result from wikipedia autocomplete search
    /// </summary>
    public sealed class WikipediaResult
    {
        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        /// <value>
        ///     The title.
        /// </value>
        public string Title { get; internal set; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; internal set; }

        /// <summary>
        ///     Gets or sets the URL.
        /// </summary>
        /// <value>
        ///     The URL.
        /// </value>
        public string Url { get; internal set; }
    }
}