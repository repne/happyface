using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Data;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Storer : IConsumerOf<Result>
    {
        private readonly IKeyValueStore<string, Result> _store;
        private readonly ITargetBlock<Result> _inner;

        private void Store(Result result)
        {
            _store.Set(result.GetKey(), result);
        }

        #region Constructors

        public Storer(IKeyValueStore<string, Result> store, Action<Result> action = null)
        {
            _store = store;
            action = action ?? Store;
            _inner = new ActionBlock<Result>(action);
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
    }
}