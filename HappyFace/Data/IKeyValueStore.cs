namespace HappyFace.Data
{
    public interface IKeyValueStore<in TKey, TValue>
    {
        TValue Get(TKey key);
        void Set(TKey key, TValue value);
    }
}