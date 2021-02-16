using System;
using System.Diagnostics;
using Elastic.Apm;
using Elastic.Apm.DiagnosticSource;

namespace Demo.Tracing
{
    public class HotChocolateDiagnosticSubscriber : IDiagnosticsSubscriber
    {
        public IDisposable Subscribe(IApmAgent agent)
        {
            if (!agent.ConfigurationReader.Enabled)
            {
                return new EmptyDisposable();
            }

            var listener = new HotChocolateDiagnosticListener();

            return DiagnosticListener.AllListeners.Subscribe(
                (IObserver<DiagnosticListener>)listener);
        }

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}