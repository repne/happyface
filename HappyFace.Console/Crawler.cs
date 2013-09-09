using System;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Data;
using HappyFace.Domain;
using HappyFace.Html;
using HappyFace.Units;

namespace HappyFace.Console
{
    public class Crawler
    {
        private readonly IKeyValueStore<string, Result> _store;
        private readonly IKeyValueStore<string, FetchTarget> _frontier;
        public Fetcher Fetcher { get; private set; }
        public Parser Parser { get; private set; }
        public Scraper Scraper { get; private set; }
        public Extractor Extractor { get; private set; }
        public Storer Storer { get; private set; }
        public Builder Builder { get; private set; }
        public Provider Provider { get; private set; }
        public Dispatcher Dispatcher { get; private set; }

        protected Crawler(IKeyValueStore<string, Result> store, IKeyValueStore<string, FetchTarget> frontier)
        {
            _store = store;
            _frontier = frontier;

            const int maxDegreeOfParallelism = 8;

            var documentFactory = new DocumentFactory();

            var fetcherOptions = new FetcherOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36",

                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            var parserOptions = new ParserOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            var scraperOptions = new ScraperOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            var extractorOptions = new ExtractorOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            //var storerOptions = new StorerOptions
            //{
            //};

            var builderOptions = new BuilderOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            var providerOptions = new ProviderOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            //var dispatcherOptions = new DispatcherOptions
            //{
            //};


            Fetcher = new Fetcher(fetcherOptions);
            Parser = new Parser(parserOptions, documentFactory);
            Scraper = new Scraper(scraperOptions);
            Extractor = new Extractor(extractorOptions);
            Storer = new Storer(store);
            Builder = new Builder(builderOptions);
            Provider = new Provider(providerOptions, store, frontier);
            Dispatcher = new Dispatcher();

            var options = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            Fetcher.LinkTo(Parser, options, x => x.StatusCode == System.Net.HttpStatusCode.OK);

            Parser.LinkTo(Scraper, options);
            Parser.LinkTo(Extractor, options);

            Fetcher.LinkTo(Builder.FetchQueue, options, x => x.StatusCode == System.Net.HttpStatusCode.OK);
            Scraper.LinkTo(Builder.ScrapeQueue, options);
            Extractor.LinkTo(Builder.ExtractQueue, options);

            Builder.LinkTo(Storer, options);

            Storer.LinkTo(Provider);
            Provider.LinkTo(Dispatcher);

            Dispatcher.LinkTo(Fetcher);
        }

        public static Crawler Create(IKeyValueStore<string, Result> store, IKeyValueStore<string, FetchTarget> frontier)
        {
            return new Crawler(store, frontier);
        }

        public void Start()
        {
            foreach (var target in _frontier.GetAll())
            {
                Dispatcher.Post(target);
            }

            Storer.Completion.Wait();
            Provider.Completion.Wait();
            Dispatcher.Completion.Wait();
        }

        public void Stop()
        {
            Provider.Complete();
        }
    }
}