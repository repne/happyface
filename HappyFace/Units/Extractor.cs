using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.Units
{
    public sealed class Extractor : IPropagatorBlock<IDocument, ExtractResponse>
    {
        private readonly IPropagatorBlock<IDocument, ExtractResponse> _inner;

        public static ExtractResponse Extract(IDocument document)
        {
            return new ExtractResponse
            {
                Paragraphs = document.Paragraphs
                                     .Where(x => x.Length > 20) //stupid heExtractorResponsestics, but works fine with my dataset, it's enough
                                     .Distinct()
                                     .ToList()
            };
        }

        #region Constructors

        public Extractor()
            : this(Extract)
        {
        }

        public Extractor(Func<IDocument, ExtractResponse> transform)
            : this(new TransformBlock<IDocument, ExtractResponse>(transform))
        {
        }

        public Extractor(IPropagatorBlock<IDocument, ExtractResponse> inner)
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

        public IDisposable LinkTo(ITargetBlock<ExtractResponse> target, DataflowLinkOptions linkOptions)
        {
            return _inner.LinkTo(target, linkOptions);
        }

        ExtractResponse ISourceBlock<ExtractResponse>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<ExtractResponse> target, out bool messageConsumed)
        {
            return _inner.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        bool ISourceBlock<ExtractResponse>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<ExtractResponse> target)
        {
            return _inner.ReserveMessage(messageHeader, target);
        }

        void ISourceBlock<ExtractResponse>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<ExtractResponse> target)
        {
            _inner.ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}