using System;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Tracing
{
    public static class RequestExecutorBuilderExtensions
    {
        public static IRequestExecutorBuilder AddObservability(
            this IRequestExecutorBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder
                .AddDiagnosticEventListener(_ => new HotChocolateDiagnosticListener());
        }
    }
}