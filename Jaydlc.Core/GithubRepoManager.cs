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
            var rateLimit = _client.GetLastApiInfo()?.RateLimit;
            if (rateLimit is null)
            {
                // Set it to some arbitrary number for now. (assumes we can still call the api)
                RateLimitRemaining = 5;
                RateLimitResetTime = null;
                return;
            }

            RateLimitRemaining = rateLimit.Remaining;
            RateLimitResetTime = rateLimit.Reset.ToLocalTime().Date;
        }

        /// <summary>
        /// A determination of whether or not we can call the github api based on the number of remaining calls and the time that our limit is reset
        /// </summary>
        private bool CanCallApi => RateLimitRemaining > 0 ||
                                   DateTime.Now > RateLimitResetTime;

        public GithubRepoManager(string userName, string cloneRoot)
        {
            _userName = userName;
            _cloneRoot = cloneRoot;
            _client = new GitHubClient(new ProductHeaderValue(userName));
            SetRateLimit();
        }

        /// <summary>
        /// Returns the list of public repositories if we are within rate limit.
        /// Returns null if otherwise
        /// </summary>
        public async Task<IReadOnlyList<ManagedRepo>?> GetRepos()
        {
            if (!CanCallApi)
            {
                return null;
            }

            var repos = await _client.Repository.GetAllForUser(_userName);
            SetRateLimit();

            return repos?.Select(x => ManagedRepo.FromGithub(_cloneRoot, x))
                        .ToImmutableList();
        }
    }
}