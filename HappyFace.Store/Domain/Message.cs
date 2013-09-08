namespace HappyFace.Store.Domain
{
    public class Message<TKey, TValue>
    {
        public Message()
        {
        }

        public Message(MessageType messageType, TKey key, TValue value)
        {
            MessageType = messageType;
            Key = key;
            Value = value;
        }

        public MessageType MessageType { get; set; }
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}