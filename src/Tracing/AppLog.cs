using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Events;

namespace Demo.Tracing
{
    internal static class AppLog
    {
        private static string EventNameProperty = "EventName";

        internal static void Write(
            string eventName, 
            LogEventLevel logEventLevel, 
            string message, 
            params object[] messageProperties)
        {
            Log.BindMessageTemplate(message, messageProperties, out var template, out var boundProperties);

            var properties = new List<LogEventProperty>
            {
                new LogEventProperty(EventNameProperty, new ScalarValue(eventName))
            };

            if (messageProperties.Length > 0)
            {
                properties.AddRange(boundProperties);
            }

            var logEvent = new LogEvent(DateTimeOffset.Now, logEventLevel, default, template, properties);

            Log.Write(logEvent);
        }
    }
}