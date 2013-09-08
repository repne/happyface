using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace HappyFace.Store.Tasks
{
    public class NeverendingTask
    {
        private readonly ITargetBlock<DateTimeOffset> _inner;

        public NeverendingTask(Func<DateTimeOffset, CancellationToken, Task> action)
            : this(action, new CancellationTokenSource().Token)
        {
        }

        public NeverendingTask(Func<DateTimeOffset, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            _inner = new ActionBlock<DateTimeOffset>(async now =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken)
                          .ConfigureAwait(false);

                await action(now, cancellationToken);

                _inner.Post(DateTimeOffset.Now);
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });
        }

        public void Start()
        {
            _inner.Post(DateTimeOffset.Now);
        }

        public void Stop()
        {
            _inner.Post(DateTimeOffset.Now);
            _inner.Complete();
            _inner.Completion.Wait();
        }
    }
}