using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Data;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Storer : IConsumerOf<Result>, IProducerOf<Result>
    {
        private readonly IKeyValueStore<string, Result> _store;
        private readonly IPropagatorBlock<Result, Result> _inner;

        private Result Store(Result result)
        {
            _store.Set(result.GetKey(), result);
            return result;
        }

        #region Constructors

        public Storer(IKeyValueStore<string, Result> store, Func<Result, Result> transform = null)
        {
            _store = store;
            transform = transform ?? Store;
            _inner = new BroadcastBlock<Result>(transform);
        }

        #endregion

        #region IConsumerOf

        public ITargetBlock<Result> Input
        {
            get
            {
                return _inner;
            }
        }

        #endregion

        #region IProducerOf

        public ISourceBlock<Result> Output
        {
            get
            {
                return _inner;
            }
        }

        #endregion
    }
}