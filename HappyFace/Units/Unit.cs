using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace HappyFace.Units
{
    public static class Unit
    {
        public static void SendTo<T>(this IProducerOf<T> source, IConsumerOf<T> target, Predicate<T> filter = null)
        {
            if (filter != null)
            {
                source.Output.LinkTo(target.Input, new DataflowLinkOptions
                {
                    PropagateCompletion = true
                }, filter);
            }
            else
            {
                source.Output.LinkTo(target.Input, new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });
            }
        }

        // not sure if I really want/need these

        public static void Init<T>(this IConsumerOf<T> target, T item)
        {
            target.Input.Post(item);
        }

        public static void Init<T>(this IConsumerOf<T> target, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                target.Input.Post(item);
            }
        }
    }
}