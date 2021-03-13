using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jaydlc.Core;
using Jaydlc.Web.GraphQL;
using Jaydlc.Web.Utils;
using Jaydlc.Web.Utils.HostedServices;
using MatBlazor;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;

namespace Jaydlc.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        private Dictionary<string, Type> GitHubProjectHandlers { get; } = new();

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var repoRootUrl =
                this.Configuration.GetValue<string>("RepoBaseUrl") ??
                throw new Exception(
                    "RepoBaseUrl configuration value not specified"
                );

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMatBlazor();

            services.AddMemoryCache();

            services.AddHttpContextAccessor();

            services.AddGraphQLServer().AddQueryType<Query>();

            // TODO: Replace concrete implementations with interfaces
            services.AddSingleton(
                new GithubRepoManager(
                    "CavemanJay", Path.Join(Constants.TempFolder, "repos")
                )
            );

            services.AddSingleton(
                new VideoManager(
                    this.Configuration.GetValue<string>("VideoInfoRoot"),
                    "PLcMVeicy89wnqOrlvFrOnljwYKGjizvx-",
                    Path.Join(Constants.TempFolder, "youtubedl")
                )
            );


#if !DEBUG
            services.AddHostedService<VideoSynchronizer>();
#endif

            var githubWebhookHandlerTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(
                    x => typeof(GithubHookHandler).IsAssignableFrom(x) &&
                         !x.IsAbstract && !x.IsInterface
                );

            var repoCloneRoot = Constants.TempFolder;

            foreach (var handlerType in githubWebhookHandlerTypes)
            {
                var instance = Activator.CreateInstance(
                    handlerType, repoRootUrl, repoCloneRoot
                );

                if (instance is null)
                    continue;

                services.AddSingleton(handlerType, instance);
                GithubHookHandler handler = (GithubHookHandler) instance;
                this.GitHubProjectHandlers[handler.RepoName] = handlerType;
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
                                           ForwardedHeaders.XForwardedProto,
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

                    options.GetLevel = (context, _, exception) =>
                    {
                        if (context.Request.Path.Value?.Contains("_blazor") ??
                            false)
                            return LogEventLevel.Debug;

                        // Implement default behavior
                        if (context.Response.StatusCode > 499 ||
                            exception is not null)
                        {
                            return LogEventLevel.Error;
                        }

                        return LogEventLevel.Information;
                    };

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

                    endpoints.HandleWebhooks(this.GitHubProjectHandlers);
                }
            );
        }
    }
}