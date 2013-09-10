using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Builder : IConsumerOf<FetchResult>,
                                  IConsumerOf<ExtractResult>,
                                  IConsumerOf<ScrapeResult>,
                                  IProducerOf<Result>
    {
        private static Result Build(Tuple<FetchResult, ExtractResult, ScrapeResult> input)
        {
            return new Result
            {
                Level = input.Item1.Level,
                LastModified = input.Item1.LastModified,
                ResponseUri = input.Item1.ResponseUri,
                Paragraphs = input.Item2.Paragraphs.ToArray(),
                Links = input.Item3.Links.ToArray()
            };
        }

        #region Fields

        private readonly ISourceBlock<Tuple<FetchResult, ExtractResult, ScrapeResult>> _input;
        private readonly IPropagatorBlock<Tuple<FetchResult, ExtractResult, ScrapeResult>, Result> _output;

        #endregion

        #region Constructors

        public Builder(BuilderOptions options, Func<Tuple<FetchResult, ExtractResult, ScrapeResult>, Result> transform = null)
        {
            transform = transform ?? Build;

            _input = new JoinBlock<FetchResult, ExtractResult, ScrapeResult>(new GroupingDataflowBlockOptions
            {
                Greedy = true
            });

            _output = new TransformBlock<Tuple<FetchResult, ExtractResult, ScrapeResult>, Result>(transform, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = -1
            });

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            _input.LinkTo(_output, linkOptions);
        }

        #endregion

        #region IConsumerOf

        private JoinBlock<FetchResult, ExtractResult, ScrapeResult> Input
        {
            get
            {
                return (JoinBlock<FetchResult, ExtractResult, ScrapeResult>) _input;
            }
        }
        
        ITargetBlock<FetchResult> IConsumerOf<FetchResult>.Input
        {
            get
            {
                return Input.Target1;
            }
        }

        ITargetBlock<ExtractResult> IConsumerOf<ExtractResult>.Input
        {
            get
            {
                return Input.Target2;
            }
        }

        ITargetBlock<ScrapeResult> IConsumerOf<ScrapeResult>.Input
        {
            get
            {
                return Input.Target3;
            }
        }

        #endregion

        #region IProducerOf

        public ISourceBlock<Result> Output
        {
            get
            {
                return _output;
            }
        }

        #endregion
    }
}