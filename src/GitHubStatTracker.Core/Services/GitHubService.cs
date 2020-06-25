using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Linq;

namespace GitHubStatTracker.Core.Services
{
    public class GitHubService
    {
        private readonly OAuthOptions _options;
        private readonly ILogger<GitHubService> _logger;
        private GitHubClient client;
        public GitHubService(ILogger<GitHubService> logger, IOptions<OAuthOptions> options)
        {
            _options = options.Value;
            _logger = logger;
            client = new GitHubClient(new ProductHeaderValue("GitHub-Stat-Tracker"), new Uri("https://github.com/"));
        }

        public void AuthClient(string accessToken)
        {
            client.Credentials = new Credentials(accessToken);
        }

        public async Task<RepositoryTrafficViewSummary> GetRepoViews(int repoId)
        {
            return await client.Repository.Traffic.GetViews(Convert.ToInt32(repoId), new RepositoryTrafficRequest(TrafficDayOrWeek.Day));
        }

        public async Task<RepositoryTrafficCloneSummary> GetRepoClones(int repoId)
        {
            return await client.Repository.Traffic.GetClones(Convert.ToInt32(repoId), new RepositoryTrafficRequest(TrafficDayOrWeek.Day));
        }

        public async Task<IReadOnlyList<Repository>> GetReposForUser(string login)
        {
            return (await client.Repository.GetAllForUser(login));
        }

        public async Task<Repository> GetRepository(int id)
        {
            return await client.Repository.Get(id);
        }
    }
}
