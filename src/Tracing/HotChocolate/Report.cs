using System;
using System.Collections.Generic;
using HotChocolate;
using Serilog.Events;
using Agent = Elastic.Apm.Agent;

namespace Demo.Tracing
{
    internal static class Report
    {
        private static string EventName = "GraphQL";

        internal static void Error(IReadOnlyList<IError> errors)
        {
            var executionSegment = Agent.Tracer.GetCurrentExecutionSegment();
            foreach (var error in errors)
            {
                var path = error.Path?.ToString();
                executionSegment.CaptureError(error.Message, path, error.GetStackFrames());
                AppLog.Write(EventName, LogEventLevel.Error, $"{error.Message} {path}");
            }
        }

        internal static void Exception(Exception exception)
        {
            var executionSegment = Agent.Tracer.GetCurrentExecutionSegment();
            executionSegment.CaptureException(exception);
            AppLog.Write(EventName, LogEventLevel.Error, $"{exception.Message}");
        }
    }
}