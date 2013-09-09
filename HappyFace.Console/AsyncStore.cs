using System;
using System.Collections.Generic;
using HappyFace.Data;
using HappyFace.Domain;
using HappyFace.Store;
using HappyFace.Store.Serialization;
using HappyFace.Store.Storage;

namespace HappyFace.Console
{
    public class AsyncStore<TValue> : IKeyValueStore<string, TValue>, IDisposable
    {
        private readonly Engine _engine;
        private readonly Collection<string, TValue> _collection;

        public AsyncStore(string name)
        {
            var serializerFactory = new MsgPckSerializerFactory();
            var storageFactory = new FileStorageFactory(serializerFactory);

            _engine = new Engine(storageFactory);
            _collection = _engine.GetCollection<string, TValue>(name);
        }

        public TValue Get(string key)
        {
            return _collection.Get(key);
        }

        public void Set(string key, TValue value)
        {
            _collection.Set(key, value);
        }

        public void Delete(string key)
        {
            _collection.Delete(key);
        }

        public bool Exists(string key)
        {
            return _collection.Exists(key);
        }

        public IEnumerable<TValue> GetAll()
        {
            return _collection.GetAll();
        }

        public void Dispose()
        {
            _engine.Shutdown();
        }
    }
}