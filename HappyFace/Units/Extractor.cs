using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.Units
{
    public sealed class Extractor : IConsumerOf<IDocument>, IProducerOf<ExtractResult>
    {
        private ExtractResult Extract(IDocument document)
        {
            return new ExtractResult
            {
                Paragraphs = document.Paragraphs
                                     .Where(x => x.Length > 20) //stupid heuristics, but works fine with my dataset, it's enough
                                     .Distinct()
                                     .ToList()
            };
        }

        #region Fields

        private readonly ExtractorOptions _options;
        private readonly IPropagatorBlock<IDocument, ExtractResult> _inner;

        #endregion

        #region Constructors

        public Extractor(ExtractorOptions options, Func<IDocument, ExtractResult> transform = null)
        {
            _options = options;
            transform = transform ?? Extract;

            _inner = new TransformBlock<IDocument, ExtractResult>(transform, new ExecutionDataflowBlockOptions
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

        public ISourceBlock<ExtractResult> Output
        {
            get
            {
                return _inner;
            }
        }

        #endregion
    }
}