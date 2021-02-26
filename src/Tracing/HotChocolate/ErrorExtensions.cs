using System.Diagnostics;
using HotChocolate;

namespace Demo.Tracing
{
    public static class ErrorExtensions
    {
        private static StackFrame[] EmptyStackFrame = new StackFrame[0];

        public static StackFrame[] GetStackFrames(this IError error)
        {
            if (error.Exception != null)
            {
                return new EnhancedStackTrace(error.Exception).GetFrames() ?? EmptyStackFrame;
            }

            return EmptyStackFrame;
        }
    }
}