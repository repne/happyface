using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HappyFace.Data
{
    public class InMemoryStore<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _inner;

        public InMemoryStore()
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

        public void Delete(TKey key)
        {
            TValue value;
            _inner.TryRemove(key, out value);
        }

        public bool Exists(TKey key)
        {
            return _inner.ContainsKey(key);
        }

        public IEnumerable<TValue> GetAll()
        {
            return _inner.Select(x => x.Value);
        }
    }
}