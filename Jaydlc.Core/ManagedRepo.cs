using System;
using System.IO;
using System.Linq;
using Jaydlc.Core.Extensions;
using LibGit2Sharp;

namespace Jaydlc.Core
{
    public class ManagedRepo
    {
        private readonly string _cloneRoot;

        private string ClonePath => Path.Join(
            this._cloneRoot, this.GithubRepo!.Name
        );

        private Repository? GitRepo { get; set; }
        private Octokit.Repository? GithubRepo { get; set; }

        public string Name =>
            this.GithubRepo?.Name ?? this.GitRepo.Name() ??
            throw new Exception("Unable to retrieve name for repository.");

        public bool IsCloned => this.GitRepo is not null;
        public bool HasRemoteInfo => this.GithubRepo is not null;
        public bool? IsFork => this.GithubRepo?.Fork;
        public string? Description => this.GithubRepo?.Description;

        public DateTime LastCommit =>
            GitRepo.Commits.OrderByDescending(x => x.Committer.When)
                   .First()
                   .Committer.When.ToLocalTime()
                   .Date;

        public string? ReadMeUrl()
        {
            var status = this.GitRepo?.RetrieveStatus("README.md");

            if (status == FileStatus.Nonexistent)
            {
                return null;
            }

            var baseUrl = this.GithubRepo?.HtmlUrl ??
                          this.GitRepo.RemoteUrl()?.Replace(".git", "");


            return baseUrl is null ? null : baseUrl + "/blob/master/README.md";
        }

        private ManagedRepo(string cloneRoot, Octokit.Repository githubRepo)
        {
            this.GithubRepo = githubRepo;

            this._cloneRoot = Path.GetFullPath(cloneRoot);

            if (Repository.IsValid(this.ClonePath))
            {
                this.GitRepo = new Repository(this.ClonePath);
            }
        }

        private ManagedRepo(Repository gitRepo)
        {
            this.GitRepo = gitRepo;
        }

        public void Clone()
        {
            if (this.IsCloned)
            {
                return;
            }

            var path = Repository.Clone(
                this.GithubRepo?.CloneUrl ?? this.GitRepo.RemoteUrl(),
                Path.Join(this._cloneRoot, this.Name)
            );
            this.GitRepo = new Repository(path);
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
            return new(cloneRoot, repo);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}