using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            // TODO: This is still loading the entire archive into memory
            var data = new List<byte>();

            await foreach (var request in requestStream.ReadAllAsync())
            {
                var content = request.Content;
                var bytes = content.ToByteArray();
                data.AddRange(bytes);
            }

            var finishedData = data.ToArray();

            var fileName = Path.Join(this._uploadPath, "archive.tar.gz");
            File.Delete(fileName);

            await File.WriteAllBytesAsync(fileName, finishedData);

            return new UploadStatus
            {
                Status = UploadStatusCode.Ok,
                FileHash = new NullableString
                    {Content = Utils.GetFileHash(fileName)},
            };
        }

        public override Task<StatusResponse> Status(StatusRequest request,
            ServerCallContext context)
        {
            // return Task.FromResult(
            //     new StatusResponse() {Status = ServerStatus.ReadyForUpload}
            // );
            throw new NotImplementedException();
        }
    }
}