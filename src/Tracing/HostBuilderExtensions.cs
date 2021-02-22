using System;
using Elastic.Apm.AspNetCore.DiagnosticListener;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Extensions.Hosting;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Demo.Tracing
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseTracing(this IHostBuilder builder)
        {
            return builder
                .UseSerilog((ctx, _, config) =>
                {
                    ConfigureLogging(config, ctx.Configuration);
                })
            .UseElasticApm(
                new AspNetCoreDiagnosticSubscriber(),
                new HttpDiagnosticsSubscriber());
        }

        private static void ConfigureLogging(
            LoggerConfiguration loggerConfiguration,
            IConfiguration configuration)
        {
            // Get ASPNETCORE_ENVIRONMENT and set ELASTIC_APM_ENVIRONMENT
            var environment = Environment.GetEnvironmentVariable("ELASTIC_APM_ENVIRONMENT");
            var applicationName = Environment.GetEnvironmentVariable("ELASTIC_APM_SERVICE_NAME");
            var applicationPart = Environment.GetEnvironmentVariable("ELASTIC_APM_SERVICE_NAME");

            var formatterConfiguration = new EcsTextFormatterConfiguration();
            formatterConfiguration.MapCustom((elasticLog, serilogLog) =>
            {
                elasticLog.Event.Reference = applicationName;
                if (serilogLog.TryGetScalarPropertyValue("0", out var value))
                {
                    elasticLog.Event.Provider = (string) value.Value;
                }

                return elasticLog;
            });

            Serilog.Debugging.SelfLog.Enable(Console.Error);

            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .Enrich.WithElasticApmCorrelationInfo()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Environment", environment)
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithProperty("ApplicationPart", applicationPart)
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    CustomFormatter = new EcsTextFormatter(formatterConfiguration),
                    IndexFormat = $"apm-logs-{environment}-{{0:yyyy.MM.dd}}",
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                    //ModifyConnectionSettings = c => c
                    //    .ApiKeyAuthentication("", "")
                    //    .EnableHttpCompression()
                });
        }
    }
}
