using System.Linq;
using HotChocolate.Execution;

namespace Demo.Tracing
{
    internal static class ExecutionResultExtensions
    {
        public static bool HasErrors(this IExecutionResult? result)
        {
            return result?.Errors?.Any() ?? false;
        }

        public static bool HasException(this IRequestContext context)
        {
            return context.Exception != null;
        }
    }
}