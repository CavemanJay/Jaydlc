using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Jaydlc.Core;
using Jaydlc.Web.GraphQL;
using Jaydlc.Web.Utils;
using MatBlazor;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

namespace Jaydlc.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private Dictionary<string, Type> GitHubProjectHandlers { get; } = new();

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var repoRootUrl = Configuration.GetValue<string>("RepoBaseUrl") ??
                              throw new Exception(
                                  "RepoBaseUrl configuration not specified"
                              );

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMatBlazor();

            services.AddGraphQLServer().AddQueryType<Query>();

            services.AddSingleton(
                sp => new VideoManager(
                    Configuration.GetValue<string>("VideoInfoRoot"),
                    "PLcMVeicy89wnqOrlvFrOnljwYKGjizvx-"
                )
            );

            var githubWebhookHandlerTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(
                    x => typeof(GithubHookHandler).IsAssignableFrom(x) &&
                         !x.IsAbstract && !x.IsInterface
                );

            var repoCloneRoot =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Environment.ExpandEnvironmentVariables("%TEMP%")
                    : "/tmp";


            foreach (var handlerType in githubWebhookHandlerTypes)
            {
                var instance = Activator.CreateInstance(
                    handlerType, repoRootUrl, repoCloneRoot
                );

                if (instance is null)
                    continue;

                services.AddSingleton(handlerType, instance);
                GithubHookHandler handler = (GithubHookHandler) instance;
                GitHubProjectHandlers[handler.RepoName] = handlerType;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                app.UseForwardedHeaders(
                    new ForwardedHeadersOptions()
                    {
                        ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                           ForwardedHeaders.XForwardedProto
                    }
                );
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSerilogRequestLogging(
                options =>
                {
                    if (env.IsProduction())
                    {
                        options.MessageTemplate =
                            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms to {IP}";
                    }

                    options.EnrichDiagnosticContext =
                        (diagnosticContext, httpContext) =>
                        {
                            diagnosticContext.Set(
                                "IP",
                                httpContext.Connection.RemoteIpAddress
                                    ?.ToString()
                            );
                        };
                }
            );

            app.UseRouting();


            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapBlazorHub();
                    endpoints.MapFallbackToPage("/_Host");

                    endpoints.MapGraphQL();

                    endpoints.HandleWebhooks(GitHubProjectHandlers);
                }
            );
        }
    }
}