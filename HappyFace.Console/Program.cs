using System;
using HappyFace.Configuration;
using HappyFace.Data;
using HappyFace.Domain;
using HappyFace.Html;
using HappyFace.Units;
using System.Threading.Tasks.Dataflow;

namespace HappyFace.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("http://www.theguardian.com/world/2013/sep/04/putin-warns-military-action-syria");

            using (var store = new AsyncStore())
            {
                StartCrawler(store, uri);
            }
        }

        private static void StartCrawler(IKeyValueStore<string, Result> store, Uri uri)
        {
            var fetcherOptions = new FetcherOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36"
            };

            var documentFactory = new DocumentFactory();

            var fetcher = new Fetcher(fetcherOptions);
            var parser = new Parser(documentFactory);
            var scraper = new Scraper();
            var extractor = new Extractor();
            var storer = new Storer(store);
            var builder = new Builder();
            var provider = new Provider(store);
            var dispatcher = new Dispatcher();

            var options = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            fetcher.LinkTo(parser, options);

            parser.LinkTo(scraper, options);
            parser.LinkTo(extractor, options);

            fetcher.LinkTo(builder.FetchQueue, options);
            scraper.LinkTo(builder.ScrapeQueue, options);
            extractor.LinkTo(builder.ExtractQueue, options);

            builder.LinkTo(storer, options);

            storer.LinkTo(provider);
            provider.LinkTo(dispatcher);

            dispatcher.LinkTo(fetcher);

            dispatcher.Post(new FetchTarget
            {
                Level = 1,
                Uri = uri
            });

            storer.Completion.Wait();
        }
    }
}
