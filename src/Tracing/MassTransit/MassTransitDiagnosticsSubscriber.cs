using System;
using System.Diagnostics;
using Elastic.Apm;
using Elastic.Apm.DiagnosticSource;

namespace Tracing.MassTransit
{
    public class MassTransitDiagnosticsSubscriber : IDiagnosticsSubscriber
    {
        public IDisposable Subscribe(IApmAgent components)
        {
            var compositeDisposable = new CompositeDisposable();

            if (!components.ConfigurationReader.Enabled)
            {
                return compositeDisposable;
            }

            var initializer = new MassTransitDiagnosticInitializer(components);
            compositeDisposable.Add(initializer);
            compositeDisposable.Add(DiagnosticListener.AllListeners.Subscribe(initializer));

            return compositeDisposable;
        }
    }
}