using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Data;
using HappyFace.Domain;
using HappyFace.Html;
using HappyFace.Units;

namespace HappyFace
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

            var documentFactory = new DocumentFactory();

            var fetcherOptions = new FetcherOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36",
            };

            var parserOptions = new ParserOptions
            {
            };

            var scraperOptions = new ScraperOptions
            {
            };

            var extractorOptions = new ExtractorOptions
            {
            };

            //var storerOptions = new StorerOptions
            //{
            //};

            var builderOptions = new BuilderOptions
            {
            };

            var providerOptions = new ProviderOptions
            {
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

            Fetcher.SendTo(Parser, x => x.StatusCode == System.Net.HttpStatusCode.OK);

            Parser.SendTo(Scraper);
            Parser.SendTo(Extractor);

            Fetcher.SendTo(Builder, x => x.StatusCode == System.Net.HttpStatusCode.OK);
            Scraper.SendTo(Builder);
            Extractor.SendTo(Builder);

            Builder.SendTo(Storer);

            //Storer.LinkTo(new ActionBlock<Result>(x =>
            //{
            //}));

            Storer.SendTo(Provider);
            Provider.SendTo(Dispatcher, x => x != null);
            Dispatcher.SendTo(Fetcher);
        }

        public static Crawler Get(IKeyValueStore<string, Result> store, IKeyValueStore<string, FetchTarget> frontier)
        {
            return new Crawler(store, frontier);
        }

        public void Start()
        {
            //TODO: Link this to the provider instead!!
            Dispatcher.Init(_frontier.GetAll());

            Fetcher.Output.Completion.Wait();
            Storer.Input.Completion.Wait();
        }

        public void Stop()
        {
            //Provider.Complete();
        }
    }
}