using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HappyFace.Store.Storage
{
    public interface IStorage
    {
        Task Write<T>(IEnumerable<T> items, CancellationToken token);
        IEnumerable<T> Read<T>();
    }
}