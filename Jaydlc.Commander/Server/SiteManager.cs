using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Jaydlc.Commander.Shared;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Jaydlc.Commander.Server
{
    public class SiteManager
    {
        private Process? SiteProcess { get; set; }

        public void StartSite(string publishedSitePath)
        {
            var filePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Jaydlc.Web.exe"
                : "Jaydlc.Web";

            var path = Path.Join(publishedSitePath, filePath);
            var startInfo = new ProcessStartInfo(path)
                {WorkingDirectory = publishedSitePath};

            this.SiteProcess = Process.Start(startInfo);
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

        public void StopSite()
        {
            this.SiteProcess?.Kill(true);
            this.SiteProcess = null;
        }
    }
}