using Elastic.Apm.Api;

namespace Demo.Tracing
{
    public static class TracerExtension
    {
        public static IExecutionSegment? GetCurrentExecutionSegment(this ITracer tracer)
        {
            var transaction = tracer.CurrentTransaction;
            return tracer.CurrentSpan ?? (IExecutionSegment)transaction;
        }
    }
}