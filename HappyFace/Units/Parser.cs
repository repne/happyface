using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.Units
{
    public sealed class Parser : IPropagatorBlock<FetchResponse, IDocument>
    {
        private readonly IDocumentFactory _documentFactory;
        private readonly IPropagatorBlock<FetchResponse, IDocument> _input;
        private readonly IPropagatorBlock<IDocument, IDocument> _output;

        private static IDocument Parse(IDocumentFactory documentFactory, FetchResponse response)
        {
            return documentFactory.Create(response);
        }

        #region Constructors

        public Parser(ParserOptions options, IDocumentFactory documentFactory, Func<FetchResponse, IDocument> transform)
            : this(options, documentFactory, new TransformBlock<FetchResponse, IDocument>(transform))
        {
        }

        public Parser(ParserOptions options, IDocumentFactory documentFactory, IPropagatorBlock<FetchResponse, IDocument> input = null)
        {
            _input = input ?? new TransformBlock<FetchResponse, IDocument>(response => Parse(documentFactory, response), new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism
            });

            _output = new BroadcastBlock<IDocument>(x => x);
            _documentFactory = documentFactory;

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion=true
            };

            _input.LinkTo(_output, linkOptions);
        }

        #endregion

        #region IDataflowBlock

        public void Complete()
        {
            _input.Complete();
        }

        void IDataflowBlock.Fault(Exception exception)
        {
            _input.Fault(exception);
        }

        public Task Completion
        {
            get
            {
                return _output.Completion;
            }
        }

        #endregion

        #region ITargetBlock

        DataflowMessageStatus ITargetBlock<FetchResponse>.OfferMessage(DataflowMessageHeader messageHeader, FetchResponse messageValue, ISourceBlock<FetchResponse> source,
            bool consumeToAccept)
        {
            return _input.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #region ISourceBlock

        public IDisposable LinkTo(ITargetBlock<IDocument> target, DataflowLinkOptions linkOptions)
        {
            return _output.LinkTo(target, linkOptions);
        }

        IDocument ISourceBlock<IDocument>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IDocument> target, out bool messageConsumed)
        {
            return _output.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        bool ISourceBlock<IDocument>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IDocument> target)
        {
            return _output.ReserveMessage(messageHeader, target);
        }

        void ISourceBlock<IDocument>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IDocument> target)
        {
            _output.ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}