using System;
using Elastic.Apm.AspNetCore.DiagnosticListener;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Extensions.Hosting;
using Elastic.Apm.Mongo;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Tracing.MassTransit;

namespace Demo.Tracing
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseObservability(this IHostBuilder builder)
        {
            return builder
                .ConfigureServices((_, services) =>
                {
                    services.AddHttpContextAccessor();
                })
                .UseSerilog((ctx, sp, config) =>
                {
                    ConfigureLogging(config, ctx.Configuration, sp.GetService<IHttpContextAccessor>());
                })
                .UseElasticApm(
                    new AspNetCoreDiagnosticSubscriber(),
                    new HttpDiagnosticsSubscriber(),
                    new MongoDiagnosticsSubscriber(),
                    new MassTransitDiagnosticsSubscriber());
        }

        private static void ConfigureLogging(
            LoggerConfiguration loggerConfiguration,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            var environment = Environment.GetEnvironmentVariable("ELASTIC_APM_ENVIRONMENT");
            var applicationName = Environment.GetEnvironmentVariable("ELASTIC_APM_SERVICE_NAME");
            var applicationPart = Environment.GetEnvironmentVariable("ELASTIC_APM_SERVICE_NAME");

            var formatterConfiguration = new EcsTextFormatterConfiguration();
            formatterConfiguration.MapHttpContext(httpContextAccessor);
            formatterConfiguration.MapCurrentThread(true);
            formatterConfiguration.MapExceptions(false);
            formatterConfiguration.MapCustom((elasticLog, serilogLog) =>
            {
                elasticLog.Event.Dataset = applicationName;

                if (serilogLog.TryGetScalarPropertyValue("EventName", out var eventName))
                {
                    elasticLog.Event.Provider = (string) eventName.Value;
                }

                return elasticLog;
            });

            Serilog.Debugging.SelfLog.Enable(Console.Error);

            var serverUrl = Environment.GetEnvironmentVariable("ELASTIC_SERVER_URL");
            var apiId = Environment.GetEnvironmentVariable("ELASTIC_API_ID");
            var apiKey = Environment.GetEnvironmentVariable("ELASTIC_API_KEY");

            var elasticsearchSinkOptions = new ElasticsearchSinkOptions(new Uri(serverUrl))
            {
                CustomFormatter = new EcsTextFormatter(formatterConfiguration),
                IndexFormat = $"apm-logs-{environment}-{{0:yyyy.MM.dd}}",
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
            };

            if (!string.IsNullOrEmpty(apiId) && !string.IsNullOrEmpty(apiKey))
            {
                elasticsearchSinkOptions.ModifyConnectionSettings = c => c
                    .ApiKeyAuthentication(apiId, apiKey)
                    .EnableHttpCompression();
            }

            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .Enrich.WithElasticApmCorrelationInfo()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Environment", environment)
                .Enrich.FromLogContext()
                .WriteTo.Elasticsearch(elasticsearchSinkOptions);
        }
    }
}
