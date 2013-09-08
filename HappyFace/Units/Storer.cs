using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Data;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Storer : IPropagatorBlock<Result, Result>
    {
        private readonly IKeyValueStore<string, Result> _store;
        private readonly IPropagatorBlock<Result, Result> _inner;

        public static Result Store(IKeyValueStore<string, Result> store, Result result)
        {
            store.Set(result.GetKey(), result);
            return result;
        }

        #region Constructors

        public Storer(IKeyValueStore<string, Result> store)
            : this(store, result => Store(store, result))
        {
        }

        public Storer(IKeyValueStore<string, Result> store, Func<Result, Result> transform)
            : this(store, new BroadcastBlock<Result>(transform))
        {
        }

        public Storer(IKeyValueStore<string, Result> store, IPropagatorBlock<Result, Result> inner)
        {
            _store = store;
            _inner = inner;
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

        public IDisposable LinkTo(ITargetBlock<Result> target, DataflowLinkOptions linkOptions)
        {
            return _inner.LinkTo(target, linkOptions);
        }

        Result ISourceBlock<Result>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Result> target, out bool messageConsumed)
        {
            return _inner.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        bool ISourceBlock<Result>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Result> target)
        {
            return _inner.ReserveMessage(messageHeader, target);
        }

        void ISourceBlock<Result>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Result> target)
        {
            _inner.ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}