using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;
using Jaydlc.Core;
using Jaydlc.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace Jaydlc.Web.Utils
{
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Registers github repo managers to handle webhook events sent from github when code is pushed or other events
        /// </summary>
        /// <param name="routeHandlers">A dictionary of handler types with the repo name as the key</param>
        /// <param name="pattern">The url pattern to handle</param>
        public static void HandleWebhooks(
            this IEndpointRouteBuilder endpointRouteBuilder,
            IReadOnlyDictionary<string, Type> routeHandlers,
            string pattern = "/gh_webhook")
        {
            endpointRouteBuilder.MapPost(
                pattern, context => HookHandler(context, routeHandlers)
            );
        }

        private static async Task HookHandler(HttpContext context,
            IReadOnlyDictionary<string, Type> routeHandlers)
        {
            var webhookEvent =
                await JsonSerializer.DeserializeAsync<GithubWebhookEvent>(
                    context.Request.Body
                );

            if (webhookEvent is null)
            {
                return;
            }

            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GithubWebhookEndpoint");

            var eventType =
                context.Request.Headers.ContainsKey("X-Github-Event")
                    ? context.Request.Headers["X-GitHub-Event"][0]
                    : null;

            if (eventType == "ping")
            {
                logger.LogInformation(
                    "Received github ping {@event}", webhookEvent
                );
                return;
            }

            // If no handler for that repo exists
            if (!routeHandlers.ContainsKey(webhookEvent.repository.name))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            if (context.RequestServices.GetRequiredService(
                routeHandlers[webhookEvent.repository.name]
            ) is GithubHookHandler handler)
            {
                handler.Logger = context.RequestServices
                    .GetRequiredService<ILogger>()
                    .ForContext("RepoHandler", handler.RepoName);
                await handler.HandleEventAsync(webhookEvent);
            }
        }
    }
}