using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Demo.Tracing;
using Elastic.Apm;
using Elastic.Apm.Api;
using MassTransit;

namespace Tracing.MassTransit
{
    internal class MassTransitDiagnosticListener : IObserver<KeyValuePair<string, object>>
    {
        private readonly IApmAgent _apmAgent;

        private readonly ConcurrentDictionary<ActivitySpanId, IExecutionSegment> _activities = 
            new ConcurrentDictionary<ActivitySpanId, IExecutionSegment>();

        public MassTransitDiagnosticListener(IApmAgent apmAgent)
        {
            _apmAgent = apmAgent;
        }

        public void OnNext(KeyValuePair<string, object?> value)
        {
            if (Activity.Current == null)
            {
                return;
            }

            switch (value.Key)
            {
                case "MassTransit.Transport.Send.Start":
                    HandleSendStart(Activity.Current, value.Value);
                    return;
                case "MassTransit.Transport.Send.Stop":
                    HandleSendStop(Activity.Current);
                    return;
                case "MassTransit.Transport.Receive.Start":
                    HandleReceiveStart(Activity.Current, value.Value);
                    return;
                case "MassTransit.Transport.Receive.Stop":
                    HandleStop(Activity.Current);
                    return;
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        private static readonly string _type = "messaging";

        private void HandleSendStart(Activity activity, object? context)
        {
            IExecutionSegment? executionSegment = _apmAgent.Tracer.GetCurrentExecutionSegment();
            if (executionSegment != null && context is SendContext sendContext)
            {
                var spanName = context is PublishContext ? "Publish" : "Send";
                spanName = $"{spanName} {sendContext.DestinationAddress.AbsolutePath}";
                var subType = sendContext.DestinationAddress.Scheme;

                var span = executionSegment.StartSpan(spanName, _type, subType, "send");
                sendContext.Headers.Set("ElasticApm", span.OutgoingDistributedTracingData.SerializeToString());
                _activities.TryAdd(activity.SpanId, span);
            }
        }

        private void HandleReceiveStart(Activity activity, object? context)
        {
            if (context is ReceiveContext receiveContext)
            {
                var rawTracingData = receiveContext.TransportHeaders.Get<string>("ElasticApm");
                var tracingData = DistributedTracingData.TryDeserializeFromString(rawTracingData);

                var transactionName = $"Receive {receiveContext.InputAddress.AbsolutePath}";
                var transaction = _apmAgent.Tracer.StartTransaction(transactionName, _type, tracingData);
                _activities.TryAdd(activity.SpanId, transaction);
            }
        }

        private void HandleSendStop(Activity activity)
        {
            if (_activities.TryRemove(activity.SpanId, out var executionSegment))
            {
                executionSegment.End();
            }
        }

        private void HandleStop(Activity activity)
        {
            if (_activities.TryRemove(activity.ParentSpanId, out var executionSegment))
            {
                executionSegment.End();
            }
        }
    }
}