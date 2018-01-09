using System;
using System.Threading;

namespace Panda.Wikipedia
{
    public interface IWikipediaService
    {
        IObservable<WikipediaResult> AutoComplete(string search, CancellationToken cancellationToken);
    }
}