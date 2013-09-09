using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Data;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Provider : IPropagatorBlock<Result, FetchTarget>
    {
        private readonly IKeyValueStore<string, Result> _store;
        private readonly IKeyValueStore<string, FetchTarget> _frontier;
        private readonly IPropagatorBlock<Result, FetchTarget> _inner;

        private IEnumerable<FetchTarget> Provide(Result result)
        {
            if (result.Level == 0)
            {
                return Enumerable.Empty<FetchTarget>();
            }

            var newLinks = result.Links
                                 .Where(uri => !_store.Exists(uri.ToString()))
                                 .ToList();

            var oldLinks = result.Links
                                 .Except(newLinks);

            var targets = newLinks.Select(uri => new FetchTarget
            {
                Level = result.Level - 1,
                Uri = uri
            }).ToList();

            foreach (var target in targets)
            {
                _frontier.Set(target.Uri.ToString(), target);
            }

            foreach (var link in oldLinks)
            {
                _frontier.Delete(link.ToString());
            }

            return targets;
        }

        #region Constructors

        public Provider(IKeyValueStore<string, Result> store,
                        IKeyValueStore<string, FetchTarget> frontier,
                        Func<Result, IEnumerable<FetchTarget>> transform)
            : this(store, frontier, new TransformManyBlock<Result, FetchTarget>(transform))
        {
        }

        public Provider(IKeyValueStore<string, Result> store,
                        IKeyValueStore<string, FetchTarget> frontier,
                        IPropagatorBlock<Result, FetchTarget> inner = null)
        {
            _store = store;
            _frontier = frontier;

            _inner = inner ?? new TransformManyBlock<Result, FetchTarget>(result => Provide(result));
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