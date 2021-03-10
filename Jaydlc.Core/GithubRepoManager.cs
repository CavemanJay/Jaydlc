using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Jaydlc.Core.Models;
using Octokit;

namespace Jaydlc.Core
{
    public class GithubRepoManager
    {
        private readonly string _userName;
        private readonly string _cloneRoot;
        private readonly GitHubClient _client;
        private int RateLimitRemaining { get; set; }
        private DateTime? RateLimitResetTime { get; set; }

        private void SetRateLimit()
        {
            var rateLimit = this._client.GetLastApiInfo()?.RateLimit;
            if (rateLimit is null)
            {
                // Set it to some arbitrary number for now. (assumes we can still call the api)
                this.RateLimitRemaining = 5;
                this.RateLimitResetTime = null;
                return;
            }

            this.RateLimitRemaining = rateLimit.Remaining;
            this.RateLimitResetTime = rateLimit.Reset.ToLocalTime().Date;
        }

        /// <summary>
        /// A determination of whether or not we can call the github api based on the number of remaining calls and the time that our limit is reset
        /// </summary>
        private bool CanCallApi =>
            this.RateLimitRemaining > 0 ||
            DateTime.Now > this.RateLimitResetTime;

        public GithubRepoManager(string userName, string cloneRoot)
        {
            this._userName = userName;
            this._cloneRoot = cloneRoot;
            this._client = new GitHubClient(new ProductHeaderValue(userName));
            this.SetRateLimit();
        }

        /// <summary>
        /// Returns the list of public repositories if we are within rate limit.
        /// Returns null if otherwise
        /// </summary>
        public async Task<IReadOnlyCollection<ManagedRepo>?> GetRepos()
        {
            if (!this.CanCallApi)
            {
                return null;
            }

            var repos =
                await this._client.Repository.GetAllForUser(this._userName);
            this.SetRateLimit();

            return repos
                   ?.Select(x => ManagedRepo.FromGithub(this._cloneRoot, x))
                   .ToImmutableList();
        }
    }
}