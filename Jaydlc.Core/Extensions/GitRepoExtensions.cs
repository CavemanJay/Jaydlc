using System.Linq;
using LibGit2Sharp;

namespace Jaydlc.Core.Extensions
{
    public static class GitRepoExtensions
    {
        public static string? Name(this Repository? repo)
        {
            return repo?.Config.GetValueOrDefault<string>("remote.origin.url")
                       .Split("/")
                       .Last()
                       .Replace(".git", "");
        }

        public static string? RemoteUrl(this Repository? repo)
        {
            return repo?.Config.GetValueOrDefault<string>("remote.origin.url");
        }
    }
}