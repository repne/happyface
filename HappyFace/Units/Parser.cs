using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.Units
{
    public sealed class Parser : IConsumerOf<FetchResult>, IProducerOf<IDocument>
    {
        private async Task<IDocument> Parse(FetchResult result)
        {
            return _documentFactory.Create(result);
        }

        #region Fields
        
        private readonly IDocumentFactory _documentFactory;
        private readonly IPropagatorBlock<FetchResult, IDocument> _input;
        private readonly IPropagatorBlock<IDocument, IDocument> _output;
        private readonly ParserOptions _options;

        #endregion

        #region Constructors

        public Parser(ParserOptions options, IDocumentFactory documentFactory, Func<FetchResult, Task<IDocument>> transform = null)
        {
            _options = options;
            _documentFactory = documentFactory;
            transform = transform ?? Parse;

            _input = new TransformBlock<FetchResult, IDocument>(transform, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = -1
            });

            _output = new BroadcastBlock<IDocument>(x => x);

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion=true
            };

            _input.LinkTo(_output, linkOptions);
        }

        #endregion

        #region IConsumerOf

        public ITargetBlock<FetchResult> Input
        {
            get
            {
                return _input;
            }
        }

        #endregion

        #region IProducerOf

        public ISourceBlock<IDocument> Output
        {
            get
            {
                return _output;
            }
        }

        #endregion
    }
}