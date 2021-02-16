using Elastic.Apm.AspNetCore.DiagnosticListener;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Extensions.Hosting;
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
}
