using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubStatTracker.Web.Models
{
    public class TrafficViewModel
    {
        public RepoViewModel Repo { get; set; }

        public List<SummaryViewModel> ViewSummary { get; set; }

        public List<SummaryViewModel> CloneSummary { get; set; }

        public List<SummaryViewModel> StarsSummary { get; set; }

        public List<SummaryViewModel> ForksSummary { get; set; }
    }

    public class RepoViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StarCount { get; set; }
        public int ForksCount { get; set; }
        public string Description { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string User { get; set; }
        public bool SyncEnabled { get; set; }
        public DateTime? SyncSince { get; set; }

        public string UserRepo { get; set; }
    }

    public class SummaryViewModel
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int Uniques { get; set; }
    }
}
