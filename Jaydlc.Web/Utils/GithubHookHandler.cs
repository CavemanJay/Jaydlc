using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jaydlc.Web.Models;

namespace Jaydlc.Web.Utils
{
    public abstract class GithubHookHandler
    {
        /// <summary>
        /// Handles a webhook event created by github
        /// </summary>
        public virtual async Task HandleEventAsync(GithubWebhookEvent webhookEvent)
        {
            if (!IsCloned)
                await CloneRepo();
        }

        public GithubHookHandler(string repositoryUrl, string clonePath)
        {
            RepositoryUrl = repositoryUrl;
            ClonePath = Directory.CreateDirectory(clonePath);
        }

        // TODO
        public abstract void GetWriteUps();

        /// <summary>
        /// Whether or not the repo has been cloned
        /// </summary>
        protected bool IsCloned =>
            ClonePath.Exists && ClonePath.GetDirectories().Any(x => x.Name == ".git");

        /// <summary>
        /// The folder on the system to clone the repo to
        /// </summary>
        public DirectoryInfo ClonePath { get; }

        /// <summary>
        /// The URL of the hosted repository (without .git)
        /// </summary>
        public string RepositoryUrl { get; }

        protected Task CloneRepo()
        {
            var processInfo =
                new ProcessStartInfo("git", $"clone {RepositoryUrl} {ClonePath.FullName}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

            var process = new Process() {StartInfo = processInfo};

            process.Start();

            return process.WaitForExitAsync();
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