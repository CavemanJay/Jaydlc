using System;
using System.Threading;
using System.Threading.Tasks;
using Jaydlc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jaydlc.Web.HostedServices
{
    public class VideoSynchronizer : BackgroundService
    {
        private readonly ILogger<VideoSynchronizer> _logger;
        private readonly VideoManager _videoManager;

        public VideoSynchronizer(ILogger<VideoSynchronizer> logger,
            VideoManager videoManager)
        {
            this._logger = logger;
            this._videoManager = videoManager;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                this._logger.LogInformation(
                    "Worker running at: {time}", DateTime.Now
                );

                await this._videoManager.DownloadPlaylistInfo();
                await Task.Delay(TimeSpan.FromHours(1.5), stoppingToken);
            }
        }
    }
}