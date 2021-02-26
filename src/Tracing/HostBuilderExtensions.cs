using System;
using System.Linq;
using Elastic.Apm.AspNetCore.DiagnosticListener;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Extensions.Hosting;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema;
using Elastic.CommonSchema.Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Sinks.Elasticsearch;

namespace Demo.Tracing
{
    public static class HostBuilderExtensions
    {
        public static void UseObservability(this IApplicationBuilder app)
        {
            app.Use(async (httpContext, next) =>
            {
                var user = httpContext.User.Identity.IsAuthenticated 
                    ? httpContext.User.Identity.Name 
                    : "anonymous";
                LogContext.PushProperty("User", user);

                var hostIp = httpContext.Connection.RemoteIpAddress.ToString();
                LogContext.PushProperty(
                    "HostIp", !string.IsNullOrWhiteSpace(hostIp) ? hostIp : "unknown");

                await next.Invoke();
            });
        }

        public static IHostBuilder UseObservability(this IHostBuilder builder)
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
            var environment = Environment.GetEnvironmentVariable("ELASTIC_APM_ENVIRONMENT");
            var applicationName = Environment.GetEnvironmentVariable("ELASTIC_APM_SERVICE_NAME");
            var applicationPart = Environment.GetEnvironmentVariable("ELASTIC_APM_SERVICE_NAME");

            var formatterConfiguration = new EcsTextFormatterConfiguration();
            formatterConfiguration.MapCustom((elasticLog, serilogLog) =>
            {
                elasticLog.Event.Reference = applicationName;
                if (serilogLog.TryGetScalarPropertyValue("EventName", out var eventName))
                {
                    var dataset = (string)eventName.Value;
                    var category = new[] { "application" };
                    var type = new[] {dataset};
                    if (elasticLog.Event != null)
                    {
                        elasticLog.Event.Dataset = dataset;

                        elasticLog.Event.Category = elasticLog.Event.Category is {Length: > 0}
                            ? elasticLog.Event.Category.Concat(category).ToArray()
                            : category;

                        elasticLog.Event.Type = elasticLog.Event.Type is {Length: > 0}
                            ? elasticLog.Event.Type.Concat(type).ToArray()
                            : type;
                    }
                    else
                    {
                        elasticLog.Event = new Event
                        {
                            Dataset = dataset, 
                            Category = category,
                            Type = type
                        };
                    }
                }

                if (serilogLog.TryGetScalarPropertyValue("HostIp", out var hostIp))
                {
                    var value = new[] {(string) hostIp.Value};
                    if (elasticLog.Host != null)
                    {
                        elasticLog.Host.Ip = value;
                    }
                    else
                    {
                        elasticLog.Host = new Host {Ip = value};
                    }
                }

                if (serilogLog.TryGetScalarPropertyValue("User", out var username))
                {
                    if (elasticLog.User != null)
                    {
                        elasticLog.User.Name = (string) username.Value;
                    }
                    else
                    {
                        elasticLog.User = new User {Name = (string) username.Value};
                    }
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
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithProperty("ApplicationPart", applicationPart)
                .Enrich.FromLogContext()
                .WriteTo.Elasticsearch(elasticsearchSinkOptions);
        }
    }
}
