using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Fetcher : IConsumerOf<FetchTarget>, IProducerOf<FetchResult>
    {
        private async Task<FetchResult> Fetch(FetchTarget target)
        {
            var request = WebRequest.CreateHttp(target.Uri);

            request.UserAgent = _options.UserAgent;

            try
            {
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            return new FetchResult
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
            catch
            {
                return new FetchResult
                {
                    Level = target.Level,
                    ResponseUri = target.Uri,
                    StatusCode = 0,
                    LastModified = DateTime.UtcNow
                };
            }
        }

        #region Fields

        private readonly FetcherOptions _options;
        private readonly IPropagatorBlock<FetchTarget, FetchResult> _input;
        private readonly IPropagatorBlock<FetchResult, FetchResult> _output;

        #endregion

        #region Constructors

        public Fetcher(FetcherOptions options, Func<FetchTarget, Task<FetchResult>> transform = null)
        {
            _options = options;
            transform = transform ?? Fetch;

            _input = new TransformBlock<FetchTarget, FetchResult>(transform, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = -1
            });

            _output = new BroadcastBlock<FetchResult>(x => x);

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            _input.LinkTo(_output, linkOptions);
        }

        #endregion

        #region IConsumerOf

        public ITargetBlock<FetchTarget> Input
        {
            get
            {
                return _input;
            }
        }

        #endregion

        #region IProducerOf

        public ISourceBlock<FetchResult> Output
        {
            get
            {
                return _output;
            }
        }

        #endregion
    }
}