using System;
using HappyFace.Data;
using HappyFace.Domain;
using HappyFace.Store;
using HappyFace.Store.Serialization;
using HappyFace.Store.Storage;

namespace HappyFace.Console
{
    public class AsyncStore : IKeyValueStore<string, Result>, IDisposable
    {
        private readonly Engine _engine;
        private readonly Collection<string, Result> _collection;

        public AsyncStore()
        {
            var serializerFactory = new MsgPckSerializerFactory();
            var storageFactory = new FileStorageFactory(serializerFactory);

            _engine = new Engine(storageFactory);
            _collection = _engine.GetCollection<string, Result>("results");
        }

        public Result Get(string key)
        {
            return _collection.Get(key);
        }

        public void Set(string key, Result value)
        {
            _collection.Set(key, value);
        }

        public bool Exists(string key)
        {
            return _collection.Exists(key);
        }

        public void Dispose()
        {
            _engine.Shutdown();
        }
    }
}