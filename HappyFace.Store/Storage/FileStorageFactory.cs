using HappyFace.Store.Serialization;

namespace HappyFace.Store.Storage
{
    public class FileStorageFactory : IStorageFactory
    {
        private readonly ISerializerFactory _serializerFactory;

        public FileStorageFactory(ISerializerFactory serializerFactory)
        {
            _serializerFactory = serializerFactory;
        }

        public IStorage Create(string path)
        {
            return new FileStorage(path, _serializerFactory);
        }
    }
}