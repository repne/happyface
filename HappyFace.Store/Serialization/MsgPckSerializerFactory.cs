namespace HappyFace.Store.Serialization
{
    public class MsgPckSerializerFactory : ISerializerFactory
    {
        public ISerializer<T> Create<T>()
        {
            return new MsgPckSerializer<T>();
        }
    }
}