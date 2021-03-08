using System;
using Elastic.Apm.GraphQL.HotChocolate;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo.Gateway
{
    public class Startup
    {
        public bool IsLocalDeployment { get; set; }
        public IConfiguration Configuration { get; }
        public const string Accounts = "accounts";
        public const string Inventory = "inventory";
        public const string Products = "products";
        public const string Reviews = "reviews";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            IsLocalDeployment = Environment.GetEnvironmentVariable("LOCAL_DEPLOYMENT") == "true";
            services.AddHttpClient(Accounts, c => c.BaseAddress = CreateUri(Accounts));
            services.AddHttpClient(Inventory, c => c.BaseAddress = CreateUri(Inventory));
            services.AddHttpClient(Products, c => c.BaseAddress = CreateUri(Products));
            services.AddHttpClient(Reviews, c => c.BaseAddress = CreateUri(Reviews));

            services
                .AddGraphQLServer()
                .AddObservability()
                .AddQueryType(d => d.Name("Query"))
                .AddRemoteSchema(Accounts, ignoreRootTypes: true)
                .AddRemoteSchema(Inventory, ignoreRootTypes: true)
                .AddRemoteSchema(Products, ignoreRootTypes: true)
                .AddRemoteSchema(Reviews, ignoreRootTypes: true)
                .AddTypeExtensionsFromFile("./Stitching.graphql");
        }


        private Uri CreateUri(string service)
        {
            if (IsLocalDeployment)
            {
                return new Uri(Configuration.GetServiceUri(service)!, "graphql");
            }

            return new Uri(new Uri($"http://{service}.apm"), "graphql");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/live", async ctx =>
                {
                    await ctx.Response.WriteAsync($"UTC {DateTime.UtcNow.ToUniversalTime()}");
                });
                endpoints
                    .MapGraphQL()
                    .WithOptions(new GraphQLServerOptions
                    {
                        Tool = { Enable = false }
                    });
            });
        }
    }
}
