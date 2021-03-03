using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Demo.Tracing;
using Elastic.Apm;
using Elastic.Apm.Api;

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
                    HandleSendStart(Activity.Current);
                    return;
                case "MassTransit.Transport.Send.Stop":
                    HandleStop(Activity.Current);
                    return;
                case "MassTransit.Transport.Receive.Start":
                    HandleReceiveStart(Activity.Current);
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

        private void HandleSendStart(Activity activity)
        {
            IExecutionSegment? executionSegment = _apmAgent.Tracer.GetCurrentExecutionSegment();
            if (executionSegment != null)
            {
                var span = executionSegment.StartSpan("Send", "messaging", "masstransit");
                span.Action = "send";
                executionSegment = span;

                _activities.TryAdd(activity.SpanId, executionSegment);
            }
        }

        private void HandleReceiveStart(Activity activity)
        {
            var transaction = _apmAgent.Tracer.StartTransaction("Receive", "messaging");

            _activities.TryAdd(activity.SpanId, transaction);
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