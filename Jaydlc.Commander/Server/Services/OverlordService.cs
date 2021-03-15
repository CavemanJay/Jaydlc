using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using Jaydlc.Protos;
using Microsoft.Extensions.Configuration;

namespace Jaydlc.Commander.Server.Services
{
    public class OverlordService : Overlord.OverlordBase
    {
        private readonly SiteManager _siteManager;
        private readonly string _uploadPath;
        private readonly string _backupsPath;
        private readonly string _sitePath;
        private readonly string _authToken;
        private const string SiteUrl = "http://localhost:5000";

        public OverlordService(SiteManager siteManager,
            IConfiguration configuration)
        {
            this._siteManager = siteManager;
            this._uploadPath = configuration.GetValue<string>("UploadPath");
            this._backupsPath = configuration.GetValue<string>("BackupsPath");
            this._sitePath =
                configuration.GetValue<string>("ArchiveExtractionPath");
            this._authToken = configuration.GetValue<string>("AuthToken");
        }

        public override async Task UpdateWebsite(UpdateRequest request,
            IServerStreamWriter<UpdateResponse> responseStream,
            ServerCallContext context)
        {
            Task SendMessage(string message)
            {
                return responseStream.WriteAsync(
                    new UpdateResponse() {Message = message}
                );
            }

            await SendMessage("Stopping website");
            await this._siteManager.StopSite(SiteUrl);

            var datePrefix = DateTime.Now.ToString("MM_dd_yyyy__hh_mm_tt");

            var archivePath = Path.Join(
                this._backupsPath, "site_" + datePrefix + ".tar.gz"
            );
            await SendMessage("Backing up current site to " + archivePath);
            this._siteManager.BackupExistingSite(this._sitePath, archivePath);

            if (request.DeleteFilesAfterBackup)
            {
                await SendMessage("Deleting files for current website");
                this._siteManager.DeleteCurrentSiteFiles(this._sitePath);
            }

            await SendMessage("Extracting new website");
            this._siteManager.ExtractNewSite(
                Path.Join(this._uploadPath, "archive.tar.gz"), this._sitePath
            );


            await SendMessage("Starting website");
            this._siteManager.StartSite(this._sitePath);
        }

        public override Task<StartSiteResponse> StartWebsite(
            StartSiteRequest request, ServerCallContext context)
        {
            this._siteManager.StartSite(this._sitePath);

            return Task.FromResult(new StartSiteResponse());
        }

        public override async Task<KillSiteResponse> KillWebsite(
            KillSiteRequest request, ServerCallContext context)
        {
            await this._siteManager.StopSite(SiteUrl);
            return new KillSiteResponse();
        }

        public override Task<VerifyResponse> Verify(VerifyRequest request,
            ServerCallContext context)
        {
            var verified = new VerifyResponse() {Verified = true};
            var unauthorized = new VerifyResponse() {Verified = false};

            var authTokenHeader =
                context.RequestHeaders.GetValue("authorization");

            if (authTokenHeader is null)
            {
                return Task.FromResult(unauthorized);
            }

            var authToken = authTokenHeader.Split(" ")[1];

            return Task.FromResult(
                authToken == this._authToken ? verified : unauthorized
            );
        }
    }
}