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
        /// <summary>
        /// The options to pass to the dotnet executable
        /// </summary>
        private readonly DotnetOptions _options;

        /// <summary>
        /// Use the tar.gz format
        /// </summary>
        public static readonly WriterOptions CompressionOptions =
            new TarWriterOptions(CompressionType.GZip, true);

        public CommanderClient()
        {
            // Initialize the configuration so we can use appsettings.json as a client
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

            // Read in the dotnet options
            this._options = new DotnetOptions(
                websiteProjectPath, publishPath, publishArgs
            );
        }

        /// <summary>
        /// Publishes the website using the dotnet executable.
        /// Publish options are specified in appsettings.json
        /// </summary>
        private void PublishWebsite()
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

        /// <summary>
        /// Uses the SharpCompress library to create a tar.gz archive of the published website
        /// </summary>
        /// <returns></returns>
        private (string newArchivePath, string archiveHash) CreateArchive()
        {
            var compressor = new Compressor(this._options.PublishPath);

            var tempFolder = Constants.TempFolder;

            var newArchivePath = compressor.CompressTo(tempFolder);

            var archiveHash = Utils.GetFileHash(newArchivePath);

            return (newArchivePath, archiveHash);
        }

        /// <summary>
        /// Uploads the website archive to the command server
        /// </summary>
        /// <param name="serviceUrl">The url of the command server (http://localhost:8080)</param>
        /// <param name="newArchivePath">The path of the archive to upload</param>
        /// <param name="archiveHash">The hash of the local copy of the archive</param>
        private async Task UploadArchive(string serviceUrl,
            string newArchivePath, string archiveHash)
        {
            // Instantiate the grpc client
            var channel = GrpcChannel.ForAddress(serviceUrl);
            var client = new Uploader.UploaderClient(channel);

            // Begin the grpc call 
            using var call = client.Upload();

            // Open the file stream of the archive
            await using var stream = File.OpenRead(newArchivePath);

            // Create a progress bar
            using (var progressBar = new ProgressBar(
                100, "Uploading archive", ConsoleColor.White
            ))
            {
                // Use a progress reporter to allow us to report percentages
                var progressReporter = progressBar.AsProgress<double>();

                // https://stackoverflow.com/a/2030971
                // Upload the archive in increments so as to not load the whole archive into memory
                var buffer = new byte[2048];
                var bytesRead = 1;
                var totalBytesRead = 0;
                var streamLength = stream.Length;
                while (bytesRead > 0)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    totalBytesRead += bytesRead;

                    // Cut off unused data once we are at the end of the file
                    if (bytesRead < 2048)
                    {
                        buffer = buffer.Take(bytesRead).ToArray();
                    }

                    // Show progress to the user
                    var progress = (double) totalBytesRead / streamLength;
                    progressReporter.Report(progress);

                    // Send the bytes to the server
                    await call.RequestStream.WriteAsync(
                        new Chunk()
                        {
                            Content = ByteString.CopyFrom(buffer),
                        }
                    );
                }

                // End the rpc call
                await call.RequestStream.CompleteAsync();
            }

            // Get the result of the upload
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

        public async Task Run(string serviceUrl)
        {
            string newArchivePath;
            string archiveHash;
            using (new Section("Compiling Website"))
            {
                this.PublishWebsite();
            }

            using (new Section("Creating Tar Archive"))
            {
                (newArchivePath, archiveHash) = this.CreateArchive();
            }

            using (new Section("Uploading File"))
            {
                await this.UploadArchive(
                    serviceUrl, newArchivePath, archiveHash
                );
            }
        }
    }
}