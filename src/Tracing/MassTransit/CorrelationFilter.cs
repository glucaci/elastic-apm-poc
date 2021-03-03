using System.Threading.Tasks;
using Demo.Tracing;
using Elastic.Apm;
using GreenPipes;
using MassTransit;

namespace Tracing.MassTransit
{
    public class CorrelationFilter<T> : IFilter<SendContext<T>> where T : class
    {
        public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
        {
            var executionSegment = Agent.Tracer.GetCurrentExecutionSegment();
            if (executionSegment != null)
            {
                //TODO: Add traceId to context
            }

            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("correlation");
        }
    }
}