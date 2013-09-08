using System;
using HappyFace.Data;
using HappyFace.Domain;
using HappyFace.Html;
using HappyFace.Store;
using HappyFace.Store.Serialization;
using HappyFace.Store.Storage;
using HappyFace.Units;
using System.Threading.Tasks.Dataflow;

namespace HappyFace.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            CrawlerExample();
            StoreExample();
        }

        private static void StoreExample()
        {
            var serializerFactory = new MsgPckSerializerFactory();
            var storageFactory = new FileStorageFactory(serializerFactory);

            var engine = new Engine(storageFactory);
            var collection = engine.GetCollection<string, int>("results");

            collection.Set("a", 1);
            collection.Set("b", 2);
            collection.Set("c", 3);

            engine.Shutdown();
        }

        private static void CrawlerExample()
        {
            var documentFactory = new DocumentFactory();
            var store = new KeyValueStore<string, Result>();

            var uri = new Uri("http://www.theguardian.com/world/2013/sep/04/putin-warns-military-action-syria");

            var fetcherOptions = new FetcherOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36"
            };

            var fetcher = new Fetcher(fetcherOptions);
            var parser = new Parser(documentFactory);
            var scraper = new Scraper();
            var extractor = new Extractor();
            var storer = new Storer(store);
            var builder = new Builder();

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

            var sink = new ActionBlock<Result>(x =>
            {
                var d = 0;
            });

            storer.LinkTo(sink, options);

            fetcher.Post(uri);
            fetcher.Complete();

            storer.Completion.Wait();

            var xasd = 0;
        }
    }
}
