using System.Threading.Tasks.Dataflow;

namespace HappyFace.Units
{
    public interface IProducerOf<out T>
    {
        ISourceBlock<T> Output { get; }
        //ISourceBlock<T> Errors { get; }
    }
}