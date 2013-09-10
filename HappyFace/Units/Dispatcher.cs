using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public class TimedQueue<T> : IEnumerable<T>, ICollection
    {
        public void Enqueue(T item)
        {
            _inner.Enqueue(GetPair(item));
        }

        public KeyValuePair<DateTimeOffset, T> Dequeue()
        {
            return _inner.Dequeue();
        }

        #region Fields

        private readonly Queue<KeyValuePair<DateTimeOffset, T>> _inner;

        #endregion

        #region Constructors

        public TimedQueue()
        {
            _inner = new Queue<KeyValuePair<DateTimeOffset, T>>();
        }

        public TimedQueue(int capacity)
        {
            _inner = new Queue<KeyValuePair<DateTimeOffset, T>>(capacity);
        }

        public TimedQueue(IEnumerable<T> collection)
        {
            _inner = new Queue<KeyValuePair<DateTimeOffset, T>>(collection.Select(GetPair));
        }

        #endregion

        #region Helpers

        private static KeyValuePair<DateTimeOffset, T> GetPair(T value)
        {
            return new KeyValuePair<DateTimeOffset, T>(DateTimeOffset.UtcNow, value);
        }

        private static KeyValuePair<DateTimeOffset, T> GetPair(DateTimeOffset key, T value)
        {
            return new KeyValuePair<DateTimeOffset, T>(key, value);
        }

        #endregion

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.Select(item => item.Value)
                         .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.Select(item => item.Value)
                         .GetEnumerator();
        }

        #endregion

        #region ICollection

        public int Count
        {
            get
            {
                return _inner.Count;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _inner.Select(item => item.Value)
                  .ToArray()
                  .CopyTo(array, index);
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection) _inner).SyncRoot;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection) _inner).IsSynchronized;
            }
        }

        #endregion
    }

    public sealed class Dispatcher : IConsumerOf<FetchTarget>, IProducerOf<FetchTarget>
    {
        // Why isn't this in the framework already?
        private static KeyValuePair<DateTimeOffset, T> GetPair<T>(DateTimeOffset key, T value)
        {
            return new KeyValuePair<DateTimeOffset, T>(key, value);
        }

        private void PushItem(FetchTarget target, string host)
        {
            Queue<FetchTarget> pushQueue;
            if (!_hosts.TryGetValue(host, out pushQueue))
            {
                _hosts[host] = pushQueue = new Queue<FetchTarget>();
                _priority.Enqueue(GetPair(DateTimeOffset.UtcNow.AddDays(-1), pushQueue));
            }

            pushQueue.Enqueue(target);
        }

        private KeyValuePair<DateTimeOffset, Queue<FetchTarget>> GetNextQueue(DateTimeOffset now, string host)
        {
            KeyValuePair<DateTimeOffset, Queue<FetchTarget>> kp;

            do
            {
                kp = _priority.Dequeue();

                if (kp.Value.Any())
                {
                    break;
                }

                if (now - kp.Key > TimeSpan.FromMinutes(5))
                {
                    _hosts.Remove(host);
                }
                else
                {
                    _priority.Enqueue(kp);
                }
            } while (!kp.Value.Any());

            return kp;
        }

        private async Task<FetchTarget> Dispatch(FetchTarget target)
        {
            var host = target.Uri.Host;
            var now = DateTimeOffset.UtcNow;

            PushItem(target, host);

            var kp = GetNextQueue(now, host);

            var pullQueue = kp.Value;
            var lastRetrievalTime = kp.Key;
            var timeSpan = now - lastRetrievalTime;

            if (timeSpan < TimeSpan.FromSeconds(10))
            {
                await Task.Delay(TimeSpan.FromSeconds(10) - timeSpan)
                          .ConfigureAwait(false);

                now = DateTimeOffset.UtcNow;
            }

            _priority.Enqueue(GetPair(now, pullQueue));

            return pullQueue.Dequeue();
        }

        #region Constructors

        public Dispatcher(DispatcherOptions options = null, Func<FetchTarget, Task<FetchTarget>> transform = null)
        {
            transform = transform ?? Dispatch;
            _options = options;
            
            _priority = new Queue<KeyValuePair<DateTimeOffset, Queue<FetchTarget>>>();
            _hosts = new Dictionary<string, Queue<FetchTarget>>();

            _inner = new TransformBlock<FetchTarget, FetchTarget>(transform, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1
            });
        }

        #endregion

        #region Fields

        private readonly DispatcherOptions _options;
        private readonly IPropagatorBlock<FetchTarget, FetchTarget> _inner;
        private readonly Queue<KeyValuePair<DateTimeOffset, Queue<FetchTarget>>> _priority;
        private readonly Dictionary<string, Queue<FetchTarget>> _hosts;

        #endregion

        #region IConsumerOf

        public ITargetBlock<FetchTarget> Input
        {
            get
            {
                return _inner;
            }
        }

        #endregion

        #region IProducerOf

        public ISourceBlock<FetchTarget> Output
        {
            get
            {
                return _inner;
            }
        }

        #endregion
    }
}