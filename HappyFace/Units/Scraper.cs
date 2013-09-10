using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.Units
{
    public sealed class Scraper : IPropagatorBlock<IDocument, ScrapeResponse>
    {
        private readonly IPropagatorBlock<IDocument, ScrapeResponse> _inner;

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

        private static ScrapeResponse Scrape(IDocument document)
        {
            var baseUri = document.BaseUri;

            return new ScrapeResponse
            {
                Links = document.Links
                                .Select(x => CreateUri(baseUri, x))
                                .Where(x => x != null)
                                .Where(x => x.Scheme == "http" || x.Scheme == "https")
                                .Where(x => x.Host == baseUri.Host)
                                .ToList()
            };
        }

        #region Constructors

        public Scraper(ScraperOptions options, Func<IDocument, ScrapeResponse> transform)
            : this(options, new TransformBlock<IDocument, ScrapeResponse>(transform))
        {
        }

        public Scraper(ScraperOptions options, IPropagatorBlock<IDocument, ScrapeResponse> inner = null)
        {
            _inner = inner ?? new TransformBlock<IDocument, ScrapeResponse>(x => Scrape(x), new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism
            });
        }

        #endregion

        #region IDataflowBlock

        public void Complete()
        {
            _inner.Complete();
        }

        void IDataflowBlock.Fault(Exception exception)
        {
            _inner.Fault(exception);
        }

        public Task Completion
        {
            get
            {
                return _inner.Completion;
            }
        }

        #endregion

        #region ITargetBlock

        DataflowMessageStatus ITargetBlock<IDocument>.OfferMessage(DataflowMessageHeader messageHeader, IDocument messageValue, ISourceBlock<IDocument> source,
            bool consumeToAccept)
        {
            return _inner.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #region ISourceBlock

        public IDisposable LinkTo(ITargetBlock<ScrapeResponse> target, DataflowLinkOptions linkOptions)
        {
            return _inner.LinkTo(target, linkOptions);
        }

        ScrapeResponse ISourceBlock<ScrapeResponse>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<ScrapeResponse> target, out bool messageConsumed)
        {
            return _inner.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        bool ISourceBlock<ScrapeResponse>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<ScrapeResponse> target)
        {
            return _inner.ReserveMessage(messageHeader, target);
        }

        void ISourceBlock<ScrapeResponse>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<ScrapeResponse> target)
        {
            _inner.ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}