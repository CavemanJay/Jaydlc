using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Jaydlc.Commander.Models;
using Jaydlc.Commander.Shared;
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

        private Metadata _headers;

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
        /// <param name="channel">The url of the command server (http://localhost:8080)</param>
        /// <param name="newArchivePath">The path of the archive to upload</param>
        /// <param name="archiveHash">The hash of the local copy of the archive</param>
        private async Task UploadArchive(ChannelBase channel,
            string newArchivePath, string archiveHash)
        {
            // Instantiate the grpc client
            var client = new Uploader.UploaderClient(channel);

            // Begin the grpc call 
            using var call = client.Upload(this._headers);

            // Open the file stream of the archive
            await using var stream = File.OpenRead(newArchivePath);


            // Create a progress bar
            var progressOptions = new ProgressBarOptions()
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true,
                ForegroundColor = ConsoleColor.Green,
                ForegroundColorDone = ConsoleColor.Gray,
            };
            Console.WriteLine("\n");
            using (var progressBar = new ProgressBar(
                100, "Upload progress", progressOptions
            ))
            {
                // Use a progress reporter to allow us to report percentages
                var progressReporter = progressBar.AsProgress<double>();

                // https://stackoverflow.com/a/2030971
                // Upload the archive in increments so as to not load the whole archive into memory

                const int bufferSize = 1_000_000; // One MB
                var buffer = new byte[bufferSize];
                var bytesRead = 1;
                var totalBytesRead = 0;
                var streamLength = stream.Length;
                while (bytesRead > 0)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    totalBytesRead += bytesRead;

                    // Cut off unused data once we are at the end of the file
                    if (bytesRead < bufferSize)
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
                Console.WriteLine(
                    "Upload unsuccessful, server responded: " +
                    response.ErrorMessage.Content
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

        private async Task TestSiteStatus(string siteUrl)
        {
            using var client = new HttpClient {BaseAddress = new Uri(siteUrl)};

            Console.WriteLine("Testing website status");

            using var response = await client.GetAsync(
                "/", HttpCompletionOption.ResponseHeadersRead
            );

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("Website is up and running!");
            }
        }

        public async Task<int> Run(string host, int port, bool tls,
            string token)
        {
            var protocol = tls ? "https" : "http";
            var overlordPort = tls ? 8081 : 8080;
            var sitePort = tls ? 5001 : 5000;
            var overlordUrl = $"{protocol}://{host}:{overlordPort}";
            this._headers = new Metadata {{"Authorization", $"Bearer {token}"}};

            using var channel = GrpcChannel.ForAddress(overlordUrl);
            var client = new Overlord.OverlordClient(channel);

            using (new Section("Verifying Connection to Server"))
            {
                var response = await client.VerifyAsync(
                    new VerifyRequest(), this._headers
                );
                var verified = response.Verified;

                if (!verified)
                {
                    Console.WriteLine(
                        "Incorrect authorization token. Unable to continue."
                    );
                    return 1;
                }
            }

            string newArchivePath;
            string archiveHash;
            using (new Section("Compiling Website"))
            {
                this.PublishWebsite();
            }

            using (new Section("Creating Tar Archive"))
            {
                (newArchivePath, archiveHash) = this.CreateArchive();
                Console.WriteLine("Archive written to " + newArchivePath);
            }

            using (new Section("Uploading Archive to Overlord"))
            {
                await this.UploadArchive(channel, newArchivePath, archiveHash);
            }

            using (new Section("Updating Website"))
            {
                using var call = client.UpdateWebsite(
                    new UpdateRequest() {DeleteFilesAfterBackup = true},
                    this._headers
                );

                await foreach (var updateResponse in call.ResponseStream
                    .ReadAllAsync())
                {
                    Console.WriteLine("Server: " + updateResponse.Message);
                }
            }

            using (new Section("Post Update Checks"))
            {
                var siteUrl = $"{protocol}://{host}:{sitePort}";
                Console.WriteLine("Waiting for site initialization...");
                await Task.Delay(5000);
                await this.TestSiteStatus(siteUrl);
            }

            return 0;
        }
    }
}