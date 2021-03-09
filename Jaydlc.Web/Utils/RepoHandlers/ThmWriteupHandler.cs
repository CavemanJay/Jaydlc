using Jaydlc.Core;

namespace Jaydlc.Web.Utils.RepoHandlers
{
    public class ThmWriteupHandler : GithubHookHandler, IWriteUpManager
    {
        public void GetWriteUps()
        {
        }

        public override string RepoName { get; } = "thm";

        public ThmWriteupHandler(string repoRootUri, string cloneRoot) : base(repoRootUri, cloneRoot)
        {
        }
    }
}