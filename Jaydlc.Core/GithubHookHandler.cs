using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Jaydlc.Core.Models;
using LibGit2Sharp;
using Serilog;
using Repository = LibGit2Sharp.Repository;

namespace Jaydlc.Core
{
    public abstract class GithubHookHandler
    {
        protected ILogger? Logger { get; private set; } = null;
        private Repository? _repository { get; set; }

        private bool _loggerInitialized = false;

        /// <summary>
        /// Sets the logger for the handler. Will not overwrite an existing logger instance.
        /// </summary>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null</exception>
        public void SetLogger(ILogger logger)
        {
            if (_loggerInitialized && Logger is not null)
            {
                return;
            }

            // ReSharper disable once ConstantConditionalAccessQualifier
            Logger = logger?.ForContext("Repository", _repository, true) ??
                     throw new ArgumentNullException(nameof(logger));
            _loggerInitialized = true;
        }

        /// <summary>
        /// Sets the logger to null to allow for a new logger to be set
        /// </summary>
        public void ClearLogger()
        {
            Logger = null;
            _loggerInitialized = false;
        }

        /// <summary>
        /// Handles a webhook event created by github
        /// </summary>
        // TODO: Find use for the webhookEvent parameter
        public void HandleEvent(GithubWebhookEvent webhookEvent)
        {
            if (!IsCloned)
            {
                Logger?.Debug(
                    "Cloning repository to {@path}", ClonePath.FullName
                );
                Repository.Clone(RepositoryUrl, ClonePath.FullName);
                _repository = new Repository(ClonePath.FullName);
                return;
            }

            var signature = new Signature(
                "Jay", "cuevasj@usf.edu", DateTimeOffset.Now
            );

            var pullOptions = new PullOptions();
            var mergeResult = Commands.Pull(
                _repository, signature, pullOptions
            );

            // This shouldn't really happen since the repo on the server should not be edited directly
            if (mergeResult.Status == MergeStatus.Conflicts)
            {
                Logger?.Error("Pull operation resulted in a merge conflict");
            }
        }

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected GithubHookHandler(string repoRootUri, string cloneRoot)
        {
            _ = RepoName ?? throw new Exception("RepoName cannot be null");

            this.RepositoryUrl = repoRootUri + this.RepoName;
            ClonePath =
                Directory.CreateDirectory(Path.Join(cloneRoot, this.RepoName));
        }

        /// <summary>
        /// Whether or not the repo has been cloned
        /// </summary>
        private bool IsCloned =>
            Repository.IsValid(ClonePath.FullName);

        /// <summary>
        /// The folder on the system to clone the repo to
        /// </summary>
        public DirectoryInfo ClonePath { get; }

        /// <summary>
        /// The URL of the hosted repository (without .git)
        /// </summary>
        public string RepositoryUrl { get; init; }

        public abstract string RepoName { get; }
    }
}