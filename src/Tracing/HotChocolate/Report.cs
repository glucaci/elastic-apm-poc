using System;
using System.Collections.Generic;
using System.Diagnostics;
using Elastic.Apm;
using HotChocolate;
using Serilog.Events;

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
                    var path = error.Path?.ToString();
                    span.CaptureError(error.Message, path, GetStackFrames(error));
                    Serilog.Log.Write(LogEventLevel.Error, $"[{{0}}] Error {path}: {error.Message}", "GraphQL");
                }
            }
            else
            {
                foreach (var error in errors)
                {
                    var path = error.Path?.ToString();
                    transaction.CaptureError(error.Message, path, GetStackFrames(error));
                    Serilog.Log.Write(LogEventLevel.Error, $"[{{0}}] Error {path}: {error.Message}", "GraphQL");
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
                Serilog.Log.Write(LogEventLevel.Error, $"[{{0}}] Exception: {exception.Message}", "GraphQL");
            }
            else
            {
                transaction.CaptureException(exception);
                Serilog.Log.Write(LogEventLevel.Error, $"[{{0}}] Exception: {exception.Message}", "GraphQL");
            }
        }
    }
}