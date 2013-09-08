using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyFace.Store.Storage;
using HappyFace.Store.Tasks;

namespace HappyFace.Store
{
    public class Engine
    {
        private readonly IStorageFactory _storageFactory;
        private readonly ConcurrentDictionary<string, ICollection> _collections;
        private readonly NeverendingTask _writer;

        public Engine(IStorageFactory storageFactory)
        {
            _storageFactory = storageFactory;
            _collections = new ConcurrentDictionary<string, ICollection>();
            _writer = new NeverendingTask(Flush);

            _writer.Start();
        }

        public void Shutdown()
        {
            _writer.Stop();
        }

        public Collection<TKey, TValue> GetCollection<TKey, TValue>(string collectionName)
        {
            return _collections.GetOrAdd(collectionName, key => CreateCollection<TKey, TValue>(collectionName)) as Collection<TKey, TValue>;
        }

        private Collection<TKey, TValue> CreateCollection<TKey, TValue>(string collectionName)
        {
            var storage = _storageFactory.Create(collectionName + ".dat");
            var history = new History<TKey, TValue>(storage);

            return new Collection<TKey, TValue>(history);
        }

        private async Task Flush(DateTimeOffset now, CancellationToken cancellationToken)
        {
            await Task.WhenAll(_collections.Select(x => x.Value.Flush(cancellationToken)));
        }
    }
}