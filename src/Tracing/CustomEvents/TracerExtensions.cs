using Elastic.Apm.Api;

namespace Demo.Tracing
{
    internal static class TracerExtensions
    {
        public static IExecutionSegment GetExecutionSegment(this ITracer tracer)
        {
            ITransaction transaction = tracer.CurrentTransaction;
            return tracer.CurrentSpan ?? (IExecutionSegment)transaction;
        }
    }
}