using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HappyFace.Store.Serialization;

namespace HappyFace.Store.Storage
{
    public class FileStorage : IStorage
    {
        private readonly string _path;
        private readonly ISerializerFactory _serializerFactory;

        public FileStorage(string path, ISerializerFactory serializerFactory)
        {
            _path = path;
            _serializerFactory = serializerFactory;
        }

        public async Task Write<T>(IEnumerable<T> items, CancellationToken token)
        {
            var serializer = _serializerFactory.Create<T>();

            using (var fs = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                foreach (var item in items)
                {
                    serializer.Serialize(fs, item);
                }
                await fs.FlushAsync(token);
            }
        }

        public IEnumerable<T> Read<T>()
        {
            var serializer = _serializerFactory.Create<T>();

            if (!File.Exists(_path))
            {
                yield break;
            }

            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                while (stream.Position != stream.Length)
                {
                    yield return serializer.Deserialize(stream);
                }
            }
        }
    }
}