using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HappyFace.Store
{
    public class Collection<TKey, TValue> : ICollection
    {
        private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
        private readonly IHistory<TKey, TValue> _history;

        public Collection(IHistory<TKey, TValue> history)
        {
            _history = history;
            _dictionary = new ConcurrentDictionary<TKey, TValue>();

            _history.Replay(_dictionary);
            _history.Clear();
        }

        public async Task Flush(CancellationToken cancellationToken)
        {
            await _history.Flush(cancellationToken);
        }

        public TValue Get(TKey key)
        {
            return _dictionary[key];
        }

        public void Set(TKey key, TValue value)
        {
            _dictionary.AddOrUpdate(key, value, (k, v) => v);
            _history.RegisterSet(key, value);
        }

        public void Delete(TKey key)
        {
            TValue value;
            _dictionary.TryRemove(key, out value);
        }

        public bool Exists(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public IEnumerable<TValue> GetAll()
        {
            return _dictionary.Select(x => x.Value);
        }
    }
}