using System;
using System.IO;
using Jaydlc.Core.Extensions;
using LibGit2Sharp;

namespace Jaydlc.Core.Models
{
    public class ManagedRepo
    {
        private readonly string _cloneRoot;
        private string ClonePath => Path.Join(_cloneRoot, GithubRepo!.Name);
        private LibGit2Sharp.Repository? GitRepo { get; set; }
        private Octokit.Repository? GithubRepo { get; set; }

        public string Name => GithubRepo?.Name ?? GitRepo.Name() ??
            throw new Exception("Unable to retrieve name for repository.");

        public bool IsCloned => GitRepo is not null;
        public bool HasRemoteInfo => GithubRepo is not null;

        public string? ReadMeUrl()
        {
            var status = GitRepo?.RetrieveStatus("README.md");

            if (status == FileStatus.Nonexistent)
            {
                return null;
            }

            var baseUrl = GithubRepo?.HtmlUrl ??
                          GitRepo.RemoteUrl()?.Replace(".git", "");


            return baseUrl is null ? null : baseUrl + "/blob/master/README.md";
        }

        private ManagedRepo(string cloneRoot, Octokit.Repository githubRepo)
        {
            this.GithubRepo = githubRepo;

            _cloneRoot = Path.GetFullPath(cloneRoot);

            if (Repository.IsValid(ClonePath))
            {
                this.GitRepo = new Repository(ClonePath);
            }
        }

        private ManagedRepo(Repository gitRepo)
        {
            this.GitRepo = gitRepo;
        }

        public void Clone()
        {
            if (IsCloned)
            {
                return;
            }

            var path = Repository.Clone(
                GithubRepo?.CloneUrl ?? GitRepo.RemoteUrl(),
                Path.Join(_cloneRoot, Name)
            );
            GitRepo = new Repository(path);
        }

        public static ManagedRepo FromClonedRepo(string repoPath)
        {
            var fullPath = Path.GetFullPath(repoPath);
            var gitRepo = new Repository(fullPath);

            return new ManagedRepo(gitRepo);
        }

        public static ManagedRepo FromGithub(string cloneRoot,
            Octokit.Repository repo)
        {
            return new ManagedRepo(cloneRoot, repo);
        }
    }
}