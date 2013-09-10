using System;
using System.Threading.Tasks.Dataflow;
using HappyFace.Units;

namespace HappyFace.Console
{
    public class Listener<T> : IConsumerOf<T>
    {
        public Listener(Action<T> action)
        {
            Input = new ActionBlock<T>(action, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = -1
            });
        }

        public ITargetBlock<T> Input { get; private set; }
    }
}