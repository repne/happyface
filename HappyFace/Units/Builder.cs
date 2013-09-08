﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using HappyFace.Domain;

namespace HappyFace.Units
{
    public class Builder : ISourceBlock<Result>
    {
        private readonly ISourceBlock<Tuple<FetchResponse, ExtractResponse, ScrapeResponse>> _input;
        private readonly IPropagatorBlock<Tuple<FetchResponse, ExtractResponse, ScrapeResponse>, Result> _output;

        private JoinBlock<FetchResponse, ExtractResponse, ScrapeResponse> Input
        {
            get
            {
                return (JoinBlock<FetchResponse, ExtractResponse, ScrapeResponse>) _input;
            }
        }

        public ITargetBlock<FetchResponse> FetchQueue
        {
            get
            {
                return Input.Target1;
            }
        }

        public ITargetBlock<ExtractResponse> ExtractQueue
        {
            get
            {
                return Input.Target2;
            }
        }

        public ITargetBlock<ScrapeResponse> ScrapeQueue
        {
            get
            {
                return Input.Target3;
            }
        }

        public static Result Build(Tuple<FetchResponse, ExtractResponse, ScrapeResponse> input)
        {
            return new Result
            {
                LastModified = input.Item1.LastModified,
                ResponseUri = input.Item1.ResponseUri,
                Paragraphs = input.Item2.Paragraphs.ToArray(),
                Links = input.Item3.Links.ToArray()
            };
        }

        #region Constructors

        public Builder()
            : this(Build)
        {
        }

        public Builder(Func<Tuple<FetchResponse, ExtractResponse, ScrapeResponse>, Result> transform)
            : this(new TransformBlock<Tuple<FetchResponse, ExtractResponse, ScrapeResponse>, Result>(transform))
        {
        }

        public Builder(IPropagatorBlock<Tuple<FetchResponse, ExtractResponse, ScrapeResponse>, Result> output)
        {
            _input = new JoinBlock<FetchResponse, ExtractResponse, ScrapeResponse>(new GroupingDataflowBlockOptions
            {
                Greedy = true
            });

            _output = output;

            var options = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            _input.LinkTo(_output, options);
        }

        #endregion

        #region IDataflowBlock

        public void Complete()
        {
            _input.Complete();
        }

        void IDataflowBlock.Fault(Exception exception)
        {
            _input.Fault(exception);
        }

        public Task Completion
        {
            get
            {
                return _output.Completion;
            }
        }

        #endregion

        #region ISourceBlock

        public IDisposable LinkTo(ITargetBlock<Result> target, DataflowLinkOptions linkOptions)
        {
            return _output.LinkTo(target, linkOptions);
        }

        Result ISourceBlock<Result>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Result> target, out bool messageConsumed)
        {
            return _output.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        bool ISourceBlock<Result>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Result> target)
        {
            return _output.ReserveMessage(messageHeader, target);
        }

        void ISourceBlock<Result>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Result> target)
        {
            _output.ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}