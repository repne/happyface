using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Data;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.Units
{
    public sealed class Provider : IPropagatorBlock<Result, FetchTarget>
    {
        private readonly IKeyValueStore<string, Result> _store;
        private readonly IPropagatorBlock<Result, FetchTarget> _inner;

        public static IEnumerable<FetchTarget> Provide(IKeyValueStore<string, Result> store, Result result)
        {
            if (result.Level == 0)
            {
                return Enumerable.Empty<FetchTarget>();
            }

            return result.Links
                         .Where(uri => !store.Exists(uri.ToString()))
                         .Select(uri => new FetchTarget
            {
                Level = result.Level - 1,
                Uri = uri
            });
        }

        #region Constructors

        public Provider(IKeyValueStore<string, Result> store)
            : this(store, result => Provide(store, result))
        {
        }


        public Provider(IKeyValueStore<string, Result> store, Func<Result, IEnumerable<FetchTarget>> transform)
            : this(store, new TransformManyBlock<Result, FetchTarget>(transform))
        {
        }

        public Provider(IKeyValueStore<string, Result> store, IPropagatorBlock<Result, FetchTarget> inner)
        {
            _inner = inner;
            _store = store;
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

        DataflowMessageStatus ITargetBlock<Result>.OfferMessage(DataflowMessageHeader messageHeader, Result messageValue, ISourceBlock<Result> source,
            bool consumeToAccept)
        {
            return _inner.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #region ISourceBlock

        public IDisposable LinkTo(ITargetBlock<FetchTarget> target, DataflowLinkOptions linkOptions)
        {
            return _inner.LinkTo(target, linkOptions);
        }

        FetchTarget ISourceBlock<FetchTarget>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<FetchTarget> target, out bool messageConsumed)
        {
            return _inner.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        bool ISourceBlock<FetchTarget>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<FetchTarget> target)
        {
            return _inner.ReserveMessage(messageHeader, target);
        }

        void ISourceBlock<FetchTarget>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<FetchTarget> target)
        {
            _inner.ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}