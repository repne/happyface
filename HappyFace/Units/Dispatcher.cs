using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Dispatcher : IConsumerOf<FetchTarget>, IProducerOf<FetchTarget>
    {
        private bool Filter(FetchTarget target)
        {
            var host = target.Uri.Host;

            var now = DateTimeOffset.Now;

            DateTimeOffset lastRetrievalTime;

            if (_hosts.TryGetValue(host, out lastRetrievalTime))
            {
                if (now - lastRetrievalTime >= TimeSpan.FromSeconds(10))
                {
                    _hosts.AddOrUpdate(host, now, (k, v) => now);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                _hosts.AddOrUpdate(host, now, (k, v) => now);
                return true;
            }
        }

        #region Constructors

        public Dispatcher(DispatcherOptions options = null, Predicate<FetchTarget> filter = null)
        {
            _options = options;

            _hosts = new ConcurrentDictionary<string, DateTimeOffset>();
            _input = new BufferBlock<FetchTarget>();
            _output = new BufferBlock<FetchTarget>();

            var buffer = new BufferBlock<FetchTarget>();

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            _input.LinkTo(_output, linkOptions, filter ?? Filter);
            _input.LinkTo(buffer, linkOptions);
            buffer.LinkTo(_input, linkOptions);
        }

        #endregion

        #region Fields

        private readonly DispatcherOptions _options;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _hosts;
        private readonly IPropagatorBlock<FetchTarget, FetchTarget> _input;
        private readonly IPropagatorBlock<FetchTarget, FetchTarget> _output;

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

        public ISourceBlock<FetchTarget> Output
        {
            get
            {
                return _output;
            }
        }

        #endregion
    }
}