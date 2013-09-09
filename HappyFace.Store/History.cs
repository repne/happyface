using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyFace.Store.Domain;
using HappyFace.Store.Storage;

namespace HappyFace.Store
{
    public class History<TKey, TValue> : IHistory<TKey, TValue>
    {
        private ConcurrentQueue<Message<TKey, TValue>> _inner;
        private readonly IStorage _storage;

        public History(IStorage storage)
        {
            _storage = storage;
            _inner = new ConcurrentQueue<Message<TKey, TValue>>();

            foreach (var message in _storage.Read<Message<TKey, TValue>>())
            {
                _inner.Enqueue(message);
            }
        }

        public void Replay(ConcurrentDictionary<TKey, TValue> dictionary)
        {
            foreach (var item in _inner.Reverse())
            {
                switch (item.MessageType)
                {
                    case MessageType.Set:
                        dictionary[item.Key] = item.Value;
                        break;

                    case MessageType.Delete:
                        TValue value;
                        dictionary.TryRemove(item.Key, out value);
                        dictionary[item.Key] = item.Value;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public void Clear()
        {
            _inner = new ConcurrentQueue<Message<TKey, TValue>>();
        }

        public async Task Flush(CancellationToken token)
        {
            var history = _inner;
            _inner = new ConcurrentQueue<Message<TKey, TValue>>();

            if (history.Count == 0)
            {
                return;
            }

            await _storage.Write(history, token);
        }

        public void RegisterSet(TKey key, TValue value)
        {
            _inner.Enqueue(new Message<TKey, TValue>(MessageType.Set, key, value));
        }

        public void RegisterDelete(TKey key)
        {
            _inner.Enqueue(new Message<TKey, TValue>(MessageType.Delete, key, default(TValue)));
        }
    }
}