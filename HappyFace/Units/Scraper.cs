using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.Units
{
    public class Scraper : IPropagatorBlock<IDocument, ScrapeResponse>
    {
        private readonly IPropagatorBlock<IDocument, ScrapeResponse> _inner;

        public static ScrapeResponse Scrape(IDocument document)
        {
            var baseUri = document.BaseUri;

            return new ScrapeResponse
            {
                Links = document.Links.Select(x => new Uri(baseUri, x))
                                .Where(x => x.Scheme == "http" || x.Scheme == "https")
                                .Where(x => x.Host == baseUri.Host)
                                .ToList()
            };
        }

        #region Constructors

        public Scraper()
            : this(Scrape)
        {
        }


        public Scraper(Func<IDocument, ScrapeResponse> transform)
            : this(new TransformBlock<IDocument, ScrapeResponse>(transform))
        {
        }

        public Scraper(IPropagatorBlock<IDocument, ScrapeResponse> inner)
        {
            _inner = inner;
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