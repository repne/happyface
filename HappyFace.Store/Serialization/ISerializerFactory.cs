namespace HappyFace.Store.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer<T> Create<T>();
    }
}