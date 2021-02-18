using System;
using System.Collections.Generic;
using System.Diagnostics;
using Elastic.Apm;
using HotChocolate;

namespace Demo.Tracing
{
    internal static class Report
    {
        internal static void Error(IReadOnlyList<IError> errors)
        {
            StackFrame[] GetStackFrames(IError error)
            {
                var stackFrames = new StackFrame[0];
                if (error.Exception != null)
                {
                    stackFrames = new EnhancedStackTrace(error.Exception).GetFrames();
                }

                return stackFrames;
            }

            var transaction = Agent.Tracer.CurrentTransaction;
            var span = Agent.Tracer.CurrentSpan;

            if (span != null)
            {
                foreach (var error in errors)
                {
                    span.CaptureError(
                        error.Message, error.Path?.ToString(), GetStackFrames(error));
                }
            }
            else
            {
                foreach (var error in errors)
                {
                    transaction.CaptureError(
                        error.Message, error.Path?.ToString(), GetStackFrames(error));
                }
            }
        }

        internal static void Exception(Exception exception)
        {
            var transaction = Agent.Tracer.CurrentTransaction;
            var span = Agent.Tracer.CurrentSpan;

            if (span != null)
            {
                span.CaptureException(exception);
            }
            else
            {
                transaction.CaptureException(exception);
            }
        }
    }
}