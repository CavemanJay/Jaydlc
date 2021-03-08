using System;
using System.Threading.Tasks;
using Jaydlc.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jaydlc.Web.Utils
{
    public static class EndpointRouteBuilderExtensions
    {
        public static void HandleWebhooks(this IEndpointRouteBuilder endpointRouteBuilder,
            string pattern = "/gh_webhook")
        {
            endpointRouteBuilder.MapPost(pattern, HookHandler);
        }

        private static async Task HookHandler(HttpContext context)
        {
            var webhookEvent =
                await JsonSerializer.DeserializeAsync<GithubWebhookEvent>(context.Request.Body);

            if (webhookEvent is null)
            {
                return;
            }

            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("GithubWebhookEndpoint");

            var eventType = context.Request.Headers.ContainsKey("X-Github-Event")
                ? context.Request.Headers["X-GitHub-Event"][0]
                : null;

            if (eventType == "ping")
            {
                logger.LogInformation("Received github ping {@event}", webhookEvent);
                return;
            }

            var responseCode = webhookEvent.repository.name switch
            {
                "thm" => StatusCodes.Status200OK,
                _ => StatusCodes.Status404NotFound,
            };

            if (responseCode == StatusCodes.Status404NotFound)
            {
                context.Response.StatusCode = responseCode;
                return;
            }

            GithubHookHandler? handler = webhookEvent.repository.name switch
            {
                "thm" => context.RequestServices.GetService<ThmWriteupHandler>(),
                _ => null
            };

            if (handler is not null)
            {
                await handler.HandleEventAsync(webhookEvent);
            }
        }
    }
}