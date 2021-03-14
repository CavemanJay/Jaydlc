using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using Jaydlc.Core;
using Jaydlc.Protos;

namespace Jaydlc.Commander.Services
{
    public class UploadService : Uploader.UploaderBase
    {
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

            var fileName = Path.Join(Constants.TempFolder, "archive.tar.gz");
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