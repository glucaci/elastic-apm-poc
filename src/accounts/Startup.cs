using System;
using Elastic.Apm.GraphQL.HotChocolate;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Extensions.Context;

namespace Demo.Accounts
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            MongoOptions dbOptions = new MongoOptions
            {
                ConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION"),
                DatabaseName = "accounts"
            };

            services
                .AddSingleton(dbOptions)
                .AddSingleton<DbContext>()
                .AddSingleton<UserRepository>()
                .AddGraphQLServer()
                .AddObservability()
                .AddQueryType<Query>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
