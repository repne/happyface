using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HappyFace.Store
{
    public interface IHistory<TKey, TValue>
    {
        Task Flush(CancellationToken token);
        void Replay(ConcurrentDictionary<TKey, TValue> dictionary);
        void Clear();

        void RegisterSet(TKey key, TValue value);
        void RegisterDelete(TKey key);
    }
}