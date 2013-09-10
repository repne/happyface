using System.Threading.Tasks.Dataflow;

namespace HappyFace.Units
{
    public interface IConsumerOf<in T>
    {
        ITargetBlock<T> Input { get; }
    }
}