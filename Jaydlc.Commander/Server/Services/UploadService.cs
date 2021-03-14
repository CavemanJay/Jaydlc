using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Jaydlc.Commander.Shared;
using Jaydlc.Protos;
using Microsoft.Extensions.Configuration;

namespace Jaydlc.Commander.Server.Services
{
    public class UploadService : Uploader.UploaderBase
    {
        private readonly string _uploadPath;

        public UploadService(IConfiguration configuration)
        {
            this._uploadPath = configuration.GetValue<string?>("UploadPath") ??
                               throw new Exception(
                                   "UploadPath configuration cannot be null"
                               );
        }

        public override async Task<UploadStatus> Upload(
            IAsyncStreamReader<Chunk> requestStream, ServerCallContext context)
        {
            var fileName = Path.Join(this._uploadPath, "archive.tar.gz");

            try
            {
                await this.HandleIncomingData(requestStream, fileName);
            }
            catch (Exception ex)
            {
                // TODO: Logging

                return new UploadStatus()
                {
                    Status = UploadStatusCode.Failed,
                    FileHash =
                        new NullableString() {Null = NullValue.NullValue},
                    ErrorMessage = new NullableString() {Content = ex.Message},
                };
            }

            return new UploadStatus
            {
                Status = UploadStatusCode.Ok,
                FileHash = new NullableString
                    {Content = Utils.GetFileHash(fileName)},
            };
        }

        private async Task HandleIncomingData(
            IAsyncStreamReader<Chunk> requestStream, string fileName)
        {
            File.Delete(fileName);
            await using var stream = File.OpenWrite(fileName);

            await foreach (var request in requestStream.ReadAllAsync())
            {
                var content = request.Content;
                var bytes = content.ToByteArray();
                stream.Write(bytes);
            }
        }
    }
}