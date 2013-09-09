﻿using System.Threading.Tasks.Dataflow;
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

            var fetcherOptions = new FetcherOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36"
            };

            var documentFactory = new DocumentFactory();

            Fetcher = new Fetcher(fetcherOptions);
            Parser = new Parser(documentFactory);
            Scraper = new Scraper();
            Extractor = new Extractor();
            Storer = new Storer(store);
            Builder = new Builder();
            Provider = new Provider(store, frontier);
            Dispatcher = new Dispatcher();

            var options = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            Fetcher.LinkTo(Parser, options);

            Parser.LinkTo(Scraper, options);
            Parser.LinkTo(Extractor, options);

            Fetcher.LinkTo(Builder.FetchQueue, options);
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