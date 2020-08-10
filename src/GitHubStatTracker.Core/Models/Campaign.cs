using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubStatTracker.Core.Models
{
    public class Campaign
    {
        public string CampaignName { get; set; }
        public string OrgName { get; set; }

        public List<Repo> Repos { get; set; }
    }

    public class Repo
    {
        public string RepoName { get; set; }
    }
}
