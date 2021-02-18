using Elastic.Apm.Api;
using HotChocolate.Execution.Instrumentation;

namespace Demo.Tracing
{
    internal class FieldActivityScope : IActivityScope
    {
        private readonly ISpan _span;

        public FieldActivityScope(ISpan span)
        {
            _span = span;
        }

        public void Dispose()
        {
            _span.End();
        }
    }
}