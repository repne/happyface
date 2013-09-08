using System.Threading;
using System.Threading.Tasks;

namespace HappyFace.Store
{
    public interface ICollection
    {
        Task Flush(CancellationToken cancellationToken);
    }
}