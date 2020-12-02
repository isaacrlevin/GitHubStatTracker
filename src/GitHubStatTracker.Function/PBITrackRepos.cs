
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubStatTracker.Function
{
    public class PBITrackRepos
    {
        private readonly BlobStorageService _blobService;
        private readonly GitHubService _gitHubService;
        private readonly TableStorageService _tableService;
        public PBITrackRepos(IOptions<OAuthOptions> options, GitHubService gitHubService, TableStorageService tableService, BlobStorageService blobService)
        {
            _tableService = tableService;
            _gitHubService = gitHubService;
            _blobService = blobService;
        }
        public async Task Run()
        {
            var client = new GitHubClient(new ProductHeaderValue("TrackRepos"));

            var basicAuth = new Credentials(Environment.GetEnvironmentVariable("GitHubToken"));
            client.Credentials = basicAuth;

            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            List<Campaign> campaigns = JsonConvert.DeserializeObject<List<Campaign>>(await _blobService.GetRepos());

            List<RepoStats> stats = new List<RepoStats>();

            foreach (Campaign campaign in campaigns)
            {
                try
                {
                    if (!string.IsNullOrEmpty(campaign.CampaignName) && !string.IsNullOrEmpty(campaign.OrgName))
                        foreach (Repo repo in campaign.Repos)
                        {
                            if (!string.IsNullOrEmpty(repo.RepoName))
                            {
                                var views = await client.Repository.Traffic.GetViews(campaign.OrgName, repo.RepoName, new RepositoryTrafficRequest(TrafficDayOrWeek.Day));
                                var clones = await client.Repository.Traffic.GetClones(campaign.OrgName, repo.RepoName, new RepositoryTrafficRequest(TrafficDayOrWeek.Day));
                                foreach (var item in views.Views)
                                {
                                    var stat = new RepoStats($"{campaign.CampaignName}{repo.RepoName}", item.Timestamp.UtcDateTime.ToShortDateString().Replace("/", ""))
                                    {
                                        OrgName = campaign.OrgName,
                                        CampaignName = campaign.CampaignName,
                                        RepoName = repo.RepoName,
                                        Date = item.Timestamp.UtcDateTime.ToShortDateString(),
                                        Views = item.Count,
                                        UniqueUsers = item.Uniques
                                    };

                                    var clone = clones.Clones.Where(a => a.Timestamp.UtcDateTime.ToShortDateString() == item.Timestamp.UtcDateTime.ToShortDateString()).FirstOrDefault();

                                    if (clone != null)
                                    {
                                        stat.Clones = clone.Count;
                                        stat.UniqueClones = clone.Uniques;
                                    }
                                    stats.Add(stat);
                                }
                                Thread.Sleep(5000);
                            }
                        }
                }
                catch (Exception e)
                { }
            }

            string tableName = "RepoStats";
            CloudTable table = await _tableService.CreateTableAsync(tableName);

            foreach (var view in stats)
            {
                Console.WriteLine("Insert an Entity.");
                await _tableService.InsertOrMergeEntityAsync(view, "RepoStats");
            }
        }
    }
}