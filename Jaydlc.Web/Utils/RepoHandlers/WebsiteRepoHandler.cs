using Jaydlc.Core;

namespace Jaydlc.Web.Utils.RepoHandlers
{
    public class WebsiteRepoHandler : GithubHookHandler
    {
        public override string RepoName { get; } = "jaydlc";

        public WebsiteRepoHandler(string repoRootUri, string cloneRoot) : base(
            repoRootUri, cloneRoot
        )
        {
        }
    }
}