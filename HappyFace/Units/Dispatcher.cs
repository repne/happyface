using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Dispatcher : IPropagatorBlock<FetchTarget, FetchTarget>
    {
        private DispatcherOptions _options;

        private readonly ConcurrentDictionary<string, DateTimeOffset> _hosts;
        
        private readonly IPropagatorBlock<FetchTarget, FetchTarget> _input;
        private readonly IPropagatorBlock<FetchTarget, FetchTarget> _output;

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
            _hosts = new ConcurrentDictionary<string, DateTimeOffset>();

            _options = options;

            _input = new BufferBlock<FetchTarget>();
            _output = new BufferBlock<FetchTarget>();

            var buffer = new BufferBlock<FetchTarget>();

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            _input.LinkTo(_output, linkOptions, filter ?? Filter);
            _input.LinkTo(buffer, linkOptions);
            buffer.LinkTo(_input);
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

        public IDisposable LinkTo(ITargetBlock<FetchTarget> target, DataflowLinkOptions linkOptions)
        {
            return _output.LinkTo(target, linkOptions);
        }

        FetchTarget ISourceBlock<FetchTarget>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<FetchTarget> target, out bool messageConsumed)
        {
            return _output.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        bool ISourceBlock<FetchTarget>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<FetchTarget> target)
        {
            return _output.ReserveMessage(messageHeader, target);
        }

        void ISourceBlock<FetchTarget>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<FetchTarget> target)
        {
            _output.ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}