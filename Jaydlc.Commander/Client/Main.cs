using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Net.Client;
using Jaydlc.Commander.Models;
using Jaydlc.Core;
using Jaydlc.Protos;
using Microsoft.Extensions.Configuration;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;
using ShellProgressBar;

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

        private (string newArchivePath, string archiveHash) CreateArchive()
        {
            var compressor = new Compressor(this._options.PublishPath);

            var tempFolder = Constants.TempFolder;

            var newArchivePath = compressor.CompressTo(tempFolder);

            var archiveHash = Utils.GetFileHash(newArchivePath);

            return (newArchivePath, archiveHash);
        }

        private async Task UploadArchive(string newArchivePath,
            string archiveHash)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:8080");
            var client = new Uploader.UploaderClient(channel);

            using var call = client.Upload();

            await using var stream = File.OpenRead(newArchivePath);

            using (var progressBar = new ProgressBar(
                100, "Uploading archive", ConsoleColor.White
            ))
            {
                var progressReporter = progressBar.AsProgress<double>();

                // https://stackoverflow.com/a/2030971
                var buffer = new byte[2048];
                var bytesRead = 1;
                var totalBytesRead = 0;
                var streamLength = stream.Length;
                while (bytesRead > 0)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    totalBytesRead += bytesRead;

                    if (bytesRead < 2048)
                    {
                        buffer = buffer.Take(bytesRead).ToArray();
                    }

                    var progress = (double) totalBytesRead / streamLength;
                    progressReporter.Report(progress);

                    await call.RequestStream.WriteAsync(
                        new Chunk()
                        {
                            Content = ByteString.CopyFrom(buffer),
                        }
                    );
                }

                await call.RequestStream.CompleteAsync();
            }

            var response = await call;

            if (response.Status != UploadStatusCode.Ok)
            {
                // TODO: Replace with actual error message
                Console.WriteLine(
                    "Upload unsuccessful, server responsed: " + "Error message"
                );
                return;
            }

            if (response.FileHash.Content != archiveHash)
            {
                Console.WriteLine(
                    "Uh oh, hash of local archive does not match hash of uploaded archive"
                );
                return;
            }

            Console.WriteLine("Archive successfully uploaded!");
        }

        public async Task Run(string host)
        {
            string newArchivePath;
            string archiveHash;
            using (new Section("Compiling Website"))
            {
                this.CompileWebsite();
            }

            using (new Section("Creating Tar Archive"))
            {
                (newArchivePath, archiveHash) = this.CreateArchive();
            }

            using (new Section("Uploading File"))
            {
                await this.UploadArchive(newArchivePath, archiveHash);
            }
        }
    }
}