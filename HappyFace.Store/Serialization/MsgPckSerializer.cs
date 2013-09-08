using System.IO;
using MsgPack.Serialization;

namespace HappyFace.Store.Serialization
{
    public class MsgPckSerializer<T> : ISerializer<T>
    {
        private readonly MessagePackSerializer<T> _inner;

        public MsgPckSerializer()
        {
            _inner = MessagePackSerializer.Create<T>();
        }

        public void Serialize(Stream stream, T item)
        {
            _inner.Pack(stream, item);
        }

        public T Deserialize(Stream stream)
        {
            return _inner.Unpack(stream);
        }
    }
}