using System.IO;

namespace HappyFace.Store.Serialization
{
    public interface ISerializer<T>
    {
        void Serialize(Stream stream, T item);
        T Deserialize(Stream stream);
    }
}