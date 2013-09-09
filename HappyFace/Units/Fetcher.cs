using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Fetcher : IPropagatorBlock<FetchTarget, FetchResponse>
    {
        private FetcherOptions _options;
        private readonly IPropagatorBlock<FetchTarget, FetchResponse> _input;
        private readonly IPropagatorBlock<FetchResponse, FetchResponse> _output;

        private static async Task<FetchResponse> Fetch(FetcherOptions options, FetchTarget target)
        {
            var request = WebRequest.CreateHttp(target.Uri);

            request.UserAgent = options.UserAgent;

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("[FETCHED]");
                        Console.ResetColor();
                        Console.WriteLine(" {0}", target.Uri);

                        return new FetchResponse
                        {
                            Level = target.Level,
                            ResponseUri = response.ResponseUri,
                            StatusCode = response.StatusCode,
                            LastModified = response.LastModified,
                            Content = await streamReader.ReadToEndAsync(),
                        };
                    }
                }
            }
        }

        #region Constructors

        public Fetcher(FetcherOptions options, Func<FetchTarget, Task<FetchResponse>> transform)
            : this(options, new TransformBlock<FetchTarget, FetchResponse>(transform))
        {
        }

        public Fetcher(FetcherOptions options, IPropagatorBlock<FetchTarget, FetchResponse> input = null)
        {
            _input = input ?? new TransformBlock<FetchTarget, FetchResponse>(x => Fetch(options, x), new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism
            });

            _options = options;
            _output = new BroadcastBlock<FetchResponse>(x => x);

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
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

        DataflowMessageStatus ITargetBlock<FetchTarget>.OfferMessage(DataflowMessageHeader messageHeader, FetchTarget messageValue, ISourceBlock<FetchTarget> source,
            bool consumeToAccept)
        {
            return _input.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #region ISourceBlock

        public IDisposable LinkTo(ITargetBlock<FetchResponse> target, DataflowLinkOptions linkOptions)
        {
            return _output.LinkTo(target, linkOptions);
        }

        FetchResponse ISourceBlock<FetchResponse>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<FetchResponse> target, out bool messageConsumed)
        {
            return _output.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        bool ISourceBlock<FetchResponse>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<FetchResponse> target)
        {
            return _output.ReserveMessage(messageHeader, target);
        }

        void ISourceBlock<FetchResponse>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<FetchResponse> target)
        {
            _output.ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}