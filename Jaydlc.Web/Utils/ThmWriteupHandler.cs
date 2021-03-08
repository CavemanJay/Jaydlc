using System.IO;

namespace Jaydlc.Web.Utils
{
    public class ThmWriteupHandler : GithubHookHandler
    {
        public string RepositoryUrl { get; }
        public DirectoryInfo ClonePath { get; }

        public override void GetWriteUps()
        {
        }

        public ThmWriteupHandler(string repositoryUrl, string clonePath) : base(repositoryUrl,
            clonePath)
        {
        }
    }
}