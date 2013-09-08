using System.Collections.Concurrent;

namespace HappyFace.Data
{
    public class KeyValueStore<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _inner;

        public KeyValueStore()
        {
            _inner = new ConcurrentDictionary<TKey,TValue>();
        }

        public TValue Get(TKey key)
        {
            return _inner[key];
        }

        public void Set(TKey key, TValue value)
        {
            _inner.AddOrUpdate(key, value, (k, v) => v);
        }
    }
}