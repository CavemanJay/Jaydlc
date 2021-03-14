using System.IO;

namespace Jaydlc.Commander.Models
{
    public record DotnetOptions(string RootWebProj, string PublishPath,
        string PublishArgs)
    {
        public string RootWebProj { get; init; } =
            Path.GetFullPath(RootWebProj);

        public string PublishPath { get; init; } =
            Path.GetFullPath(PublishPath);
    }
}