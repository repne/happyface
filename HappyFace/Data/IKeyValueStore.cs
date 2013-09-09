using System;
using System.Collections;
using System.Collections.Generic;

namespace HappyFace.Data
{
    public interface IKeyValueStore<in TKey, TValue>
    {
        TValue Get(TKey key);
        void Set(TKey key, TValue value);
        void Delete(TKey key);
        bool Exists(TKey key);
        IEnumerable<TValue> GetAll();
    }
}