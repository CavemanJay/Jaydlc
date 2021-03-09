using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jaydlc.Core.Exceptions;
using Jaydlc.Core.Models;
using Serilog;

namespace Jaydlc.Core
{
    public abstract class GithubHookHandler
    {
        public ILogger? Logger { get; set; }

        /// <summary>
        /// Handles a webhook event created by github
        /// </summary>
        public virtual async Task HandleEventAsync(
            GithubWebhookEvent webhookEvent)
        {
            if (!IsCloned)
                await CloneRepo();
        }

        protected GithubHookHandler(string repoRootUri, string cloneRoot)
        {
            this.RepositoryUrl = repoRootUri + this.RepoName;
            ClonePath =
                Directory.CreateDirectory(Path.Join(cloneRoot, this.RepoName));
        }

        /// <summary>
        /// Whether or not the repo has been cloned
        /// </summary>
        private bool IsCloned =>
            ClonePath.Exists &&
            ClonePath.GetDirectories().Any(x => x.Name == ".git");

        /// <summary>
        /// The folder on the system to clone the repo to
        /// </summary>
        public DirectoryInfo ClonePath { get; }

        /// <summary>
        /// The URL of the hosted repository (without .git)
        /// </summary>
        public string RepositoryUrl { get; init; }

        public abstract string RepoName { get; }

        /// <summary>
        /// Clones the repository to <see cref="ClonePath"/> based on the <see cref="RepositoryUrl"/>
        /// </summary>
        /// <exception cref="ExeNotFoundException">Git is not found in path</exception>
        protected Task CloneRepo()
        {
            var processInfo = new ProcessStartInfo(
                "git", $"clone {RepositoryUrl} {ClonePath.FullName}"
            )
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var process = new Process() {StartInfo = processInfo};

            Logger?.Debug(
                "Cloning {repository} to {path}", RepoName, ClonePath.FullName
            );
            process.Start();

            try
            {
                return process.WaitForExitAsync();
            }
            catch (Win32Exception ex)
            {
                if (ex.Message.Contains("No such file"))
                {
                    throw new ExeNotFoundException("git");
                }

                throw;
            }
        }

        protected Task PullChanges()
        {
            var processInfo = new ProcessStartInfo("git", "pull")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false, WorkingDirectory = ClonePath.FullName
            };

            var process = new Process {StartInfo = processInfo};
            process.Start();

            return process.WaitForExitAsync();
        }
    }
}