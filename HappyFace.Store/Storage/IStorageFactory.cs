namespace HappyFace.Store.Storage
{
    public interface IStorageFactory
    {
        IStorage Create(string path);
    }
}