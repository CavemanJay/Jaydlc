using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jaydlc.Commander.Server.Services
{
    public class SiteManagerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly SiteManager _siteManager;

        public SiteManagerService(IConfiguration configuration,
            SiteManager siteManager)
        {
            this._configuration = configuration;
            this._siteManager = siteManager;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Start the site if it has stopped for some reason
                if (!this._siteManager.SiteRunning &&
                    this._siteManager.SiteShouldBeRunning)
                {
                    Console.WriteLine("Starting website");

                    // TODO: Log that the site has been started and notify me via email
                    this._siteManager.StartSite(
                        this._configuration.GetValue<string>(
                            "ArchiveExtractionPath"
                        )
                    );
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}