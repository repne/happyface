﻿using System;
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
            var documentFactory = new DocumentFactory();
            var store = new KeyValueStore<string, Result>();

            var uri = new Uri("http://www.theguardian.com/world/2013/sep/04/putin-warns-military-action-syria");

            var fetcher = new Fetcher();
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
