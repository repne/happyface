using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Fetcher : IPropagatorBlock<Uri, FetchResponse>
    {
        private readonly IPropagatorBlock<Uri, FetchResponse> _input;
        private readonly IPropagatorBlock<FetchResponse, FetchResponse> _output;

        public static async Task<FetchResponse> Fetch(Uri uri)
        {
            var request = WebRequest.CreateHttp(uri);

            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36";

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        return new FetchResponse
                        {
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

        public Fetcher()
            : this(Fetch)
        {
        }


        public Fetcher(Func<Uri, Task<FetchResponse>> transform)
            : this(new TransformBlock<Uri, FetchResponse>(transform))
        {
        }

        public Fetcher(IPropagatorBlock<Uri, FetchResponse> input)
        {
            _input = input;
            _output = new BroadcastBlock<FetchResponse>(x => x);

            var options = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            _input.LinkTo(_output, options);
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

        DataflowMessageStatus ITargetBlock<Uri>.OfferMessage(DataflowMessageHeader messageHeader, Uri messageValue, ISourceBlock<Uri> source,
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