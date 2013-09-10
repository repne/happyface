using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.Units
{
    public sealed class Scraper : IConsumerOf<IDocument>, IProducerOf<ScrapeResult>
    {
        private static Uri CreateUri(Uri baseUri, Uri relativeUri)
        {
            Uri uri;
            if (Uri.TryCreate(baseUri, relativeUri, out uri))
            {
                return uri;
            }
            else
            {
                return null;
            }
        }

        private async Task<ScrapeResult> Scrape(IDocument document)
        {
            var baseUri = document.BaseUri;

            return new ScrapeResult
            {
                Links = document.Links
                                .Select(x => CreateUri(baseUri, x))
                                .Where(x => x != null)
                                .Where(x => x.Scheme == "http" || x.Scheme == "https")
                                .Where(x => x.Host == baseUri.Host)
                                .ToList()
            };
        }

        #region Fields

        private readonly IPropagatorBlock<IDocument, ScrapeResult> _inner;

        #endregion

        #region Constructors

        public Scraper(ScraperOptions options, Func<IDocument, Task<ScrapeResult>> transform = null)
        {
            transform = transform ?? Scrape;

            _inner = new TransformBlock<IDocument, ScrapeResult>(transform, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = -1
            });
        }

        #endregion

        #region IConsumerOf

        public ITargetBlock<IDocument> Input
        {
            get
            {
                return _inner;
            }
        }

        #endregion

        #region IProducerOf

        public ISourceBlock<ScrapeResult> Output
        {
            get
            {
                return _inner;
            }
        }

        #endregion
    }
}