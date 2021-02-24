using Elastic.Apm.Api;

namespace Demo.Tracing
{
    internal static class TracerExtension
    {
        internal static IExecutionSegment GetCurrentExecutionSegment(this ITracer tracer)
        {
            var transaction = tracer.CurrentTransaction;
            return tracer.CurrentSpan ?? (IExecutionSegment)transaction;
        }
    }
}