using System;
using Elastic.Apm.AspNetCore.DiagnosticListener;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Extensions.Hosting;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Demo.Tracing
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseTracing(this IHostBuilder builder)
        {
            ConfigureLogging();

            return builder
                .UseSerilog()
                .UseElasticApm(
                    new AspNetCoreDiagnosticSubscriber(),
                    new HttpDiagnosticsSubscriber());
        }

        private static void ConfigureLogging()
        {
            var environment = Environment.GetEnvironmentVariable("ELASTIC_APM_ENVIRONMENT");

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithElasticApmCorrelationInfo()
                .Enrich.WithMachineName()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                    new Uri("http://localhost:9200"))
                {
                    CustomFormatter = new EcsTextFormatter()
                })
                .Enrich.WithProperty("Environment", environment)
                .CreateLogger();
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

            builder.AddDiagnosticEventListener(_ => new HotChocolateDiagnosticListener());

            return builder;
        }
    }
}
