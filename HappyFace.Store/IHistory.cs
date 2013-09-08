using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HappyFace.Store
{
    public interface IHistory<TKey, TValue>
    {
        Task Flush(CancellationToken token);
        void RegisterSet(TKey key, TValue value);
        void Replay(IDictionary<TKey, TValue> dictionary);
        void Clear();
    }
}