using System;
using System.Diagnostics;
using System.IO;
using Jaydlc.Commander.Models;
using Jaydlc.Core;
using Microsoft.Extensions.Configuration;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;

namespace Jaydlc.Commander.Client
{
    public class CommanderClient
    {
        private readonly DotnetOptions _options;

        public static readonly WriterOptions CompressionOptions =
            new TarWriterOptions(CompressionType.GZip, true);

        public CommanderClient()
        {
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile(
                                    "appsettings.json", optional: false,
                                    reloadOnChange: true
                                )
                                .Build();

            const string projTag = "RootWebProj";

            var websiteProjectPath = Path.GetFullPath(
                configuration.GetValue<string>(projTag)
            );

            var publishPath = Path.GetFullPath(
                configuration.GetValue<string>("PublishPath")
            );

            var publishArgs = configuration.GetValue<string>("PublishArgs")
                                           .Replace(
                                               "$" + projTag, websiteProjectPath
                                           )
                                           .Replace(
                                               "$PublishPath", publishPath
                                           );

            this._options = new DotnetOptions(
                websiteProjectPath, publishPath, publishArgs
            );
        }

        private void CompileWebsite()
        {
            var startInfo = new ProcessStartInfo(
                "dotnet", this._options.PublishArgs
            )
            {
                RedirectStandardOutput = false,
                RedirectStandardError = false,
            };

            var p = new Process() {StartInfo = startInfo};
            p.Start();

            p.WaitForExit();
        }

        public void Run(string host)
        {
            using (new Section("Compiling Website"))
            {
                this.CompileWebsite();
            }

            using (new Section("Creating Tar Archive"))
            {
                var compressor = new Compressor(this._options.PublishPath);

                var tempFolder = Constants.TempFolder;

                compressor.CompressTo(tempFolder);
            }
        }
    }
}