using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Configuration;
using HappyFace.Data;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public sealed class Provider : IConsumerOf<Result>, IProducerOf<FetchTarget>
    {
        private async Task<IEnumerable<FetchTarget>> Provide(Result result)
        {
            if (result.Level == 0)
            {
                return Enumerable.Empty<FetchTarget>();
            }

            var newLinks = result.Links
                                 .Where(uri => !_store.Exists(uri.ToString()))
                                 .ToList();

            var oldLinks = result.Links
                                 .Except(newLinks);

            var targets = newLinks.Select(uri => new FetchTarget
            {
                Level = result.Level - 1,
                Uri = uri
            }).ToList();

            foreach (var target in targets)
            {
                _frontier.Set(target.Uri.ToString(), target);
            }

            foreach (var link in oldLinks)
            {
                _frontier.Delete(link.ToString());
            }

            return targets;
        }

        #region Fields

        private readonly ProviderOptions _options;
        private readonly IKeyValueStore<string, Result> _store;
        private readonly IKeyValueStore<string, FetchTarget> _frontier;
        private readonly IPropagatorBlock<Result, FetchTarget> _inner;

        #endregion

        #region Constructors

        public Provider(ProviderOptions options,
                        IKeyValueStore<string, Result> store,
                        IKeyValueStore<string, FetchTarget> frontier,
                        Func<Result, Task<IEnumerable<FetchTarget>>> transform = null)
        {
            _options = options;
            _store = store;
            _frontier = frontier;
            transform = transform ?? Provide;

            _inner = new TransformManyBlock<Result, FetchTarget>(transform, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = -1
            });
        }

        #endregion

        #region IConsumerOf

        public ITargetBlock<Result> Input
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