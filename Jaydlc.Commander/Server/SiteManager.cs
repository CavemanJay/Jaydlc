using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Jaydlc.Commander.Shared;
using Serilog;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Jaydlc.Commander.Server
{
    public class SiteManager
    {
        private readonly ILogger _logger;
        private Process? SiteProcess { get; set; }
        private CancellationTokenSource _tokenSource;

        public bool SiteShouldBeRunning =>
            !this._tokenSource.IsCancellationRequested;

        public bool SiteRunning => !this.SiteProcess?.HasExited ?? false;

        public SiteManager(ILogger logger)
        {
            this._logger = logger;
            this._tokenSource = new CancellationTokenSource();
        }

        public void StartSite(string publishedSitePath)
        {
            if (this.SiteProcess is not null && !this.SiteProcess.HasExited)
                throw new Exception(
                    "An existing site process is already running"
                );

            var filePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Jaydlc.Web.exe"
                : "Jaydlc.Web";

            var path = Path.Join(publishedSitePath, filePath);
            var startInfo = new ProcessStartInfo(path)
                {WorkingDirectory = publishedSitePath};

            try
            {
                this.SiteProcess = Process.Start(startInfo);

                this._tokenSource = new CancellationTokenSource();
            }
            catch (Win32Exception ex) when (ex.Message.Contains("No such file"))
            {
                this._logger.Error(ex, "Unable to start website");
            }
        }

        public void BackupExistingSite(string sitePath, string archivePath)
        {
            var compressor = new Compressor(sitePath);
            compressor.CompressTo(archivePath);
        }

        public void DeleteCurrentSiteFiles(string sitePath)
        {
            var files = Directory.GetFiles(sitePath);
            foreach (var file in files)
            {
                File.Delete(file);
            }

            var folders = Directory.GetDirectories(sitePath);
            foreach (var folder in folders)
            {
                Directory.Delete(folder, true);
            }
        }

        public void ExtractNewSite(string uploadedArchivePath, string sitePath)
        {
            using var stream = File.OpenRead(uploadedArchivePath);
            using var reader = ReaderFactory.Open(stream);

            // Archive agnostic extraction
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    reader.WriteEntryToDirectory(
                        sitePath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true,
                        }
                    );
                }
            }
        }

        public async Task StopSite(string siteUrl)
        {
            if (this.SiteProcess is null)
                return;

            using var client = new HttpClient();
            await client.PostAsync(
                siteUrl + "/shutdown", new StringContent("")
            );

            this._tokenSource.Cancel();
            await this.SiteProcess.WaitForExitAsync();

            this.SiteProcess = null;
        }
    }
}