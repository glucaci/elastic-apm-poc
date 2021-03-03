using System;
using System.Diagnostics;
using Elastic.Apm;

namespace Tracing.MassTransit
{
    public class MassTransitDiagnosticInitializer : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly IApmAgent _apmAgent;
        private IDisposable? _sourceSubscription;

        internal MassTransitDiagnosticInitializer(IApmAgent apmAgent)
        {
            _apmAgent = apmAgent;
        }

        public void Dispose() => _sourceSubscription?.Dispose();
        
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == "MassTransit")
                _sourceSubscription = value.Subscribe(new MassTransitDiagnosticListener(_apmAgent));
        }
    }
}