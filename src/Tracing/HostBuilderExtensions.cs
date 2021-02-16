using System;
using Elastic.Apm.AspNetCore.DiagnosticListener;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Extensions.Hosting;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo.Tracing
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseTracing(this IHostBuilder builder)
        {
            return builder.UseElasticApm(
                new HttpDiagnosticsSubscriber(),
                new AspNetCoreDiagnosticSubscriber());
        }
    }

    public static class HotChocolateRequestExecutorBuilderExtensions
    {
        public static IRequestExecutorBuilder AddTracing(
            this IRequestExecutorBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddDiagnosticEventListener(sp => new HotChocolateDiagnosticListener());

            return builder;
        }
    }
}
