
using GitHubStatTracker.Core.Models;
using GitHubStatTracker.Core.Services;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubStatTracker.Function
{
    public class TrackRepos
    {
        private readonly GitHubService _gitHubService;
        private readonly TableStorageService _tableService;
        public TrackRepos(IOptions<OAuthOptions> options, GitHubService gitHubService, TableStorageService tableService)
        {
            _tableService = tableService;
            _gitHubService = gitHubService;
        }
        public async Task Run()
        {
            List<RepoStatEntity> stats = new List<RepoStatEntity>();
            var repos = await _tableService.GetActiveRepos();


            List<string> dateArray = new List<string>();
            for (DateTime date = DateTime.Now.AddDays(-10); date <= DateTime.Now; date = date.AddDays(1))
            {
                dateArray.Add(date.ToShortDateString());
            }


            foreach (var repo in repos.Where(a => dateArray.Contains(a.Date)))
            {
                _gitHubService.AuthClient(repo.AccessToken);

                var todayRepo = await _gitHubService.GetRepository(repo.RepoId);

                var views = await _gitHubService.GetRepoViews(repo.RepoId);
                var clones = await _gitHubService.GetRepoClones(repo.RepoId);
                foreach (var view in views.Views)
                {
                    var stat = new RepoStatEntity($"{repo.UserName}{repo.RepoName}", view.Timestamp.DateTime.ToShortDateString().Replace("/", ""))
                    {
                        AccessToken = repo.AccessToken,
                        RepoName = repo.RepoName,
                        Date = view.Timestamp.DateTime.ToShortDateString(),
                        RepoId = repo.RepoId,
                        UserName = repo.UserName,
                        SyncEnabled = repo.SyncEnabled,

                        Views = view.Count,
                        UniqueUsers = view.Uniques
                    };

                    if (stat.Date == DateTime.Now.ToShortDateString())
                    {
                        stat.StarCount = todayRepo.StargazersCount;
                        stat.ForksCount = todayRepo.ForksCount;
                    }
                    else
                    {
                        stat.ForksCount = repo.ForksCount;
                        stat.StarCount = repo.StarCount;
                    }


                    var clone = clones.Clones.Where(a => a.Timestamp.DateTime.ToShortDateString() == view.Timestamp.DateTime.ToShortDateString()).FirstOrDefault();

                    if (clone != null)
                    {
                        stat.Clones = clone.Count;
                        stat.UniqueClones = clone.Uniques;
                    }
                    stats.Add(stat);
                }
            }

            foreach (var view in stats)
            {
                await _tableService.InsertOrMergeEntityAsync(view);
            }
        }
    }
}