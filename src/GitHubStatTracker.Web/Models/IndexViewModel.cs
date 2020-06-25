using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
namespace GitHubStatTracker.Web.Models
{
    public class IndexViewModel
    {
        public List<RepoViewModel> Repos { get; set; }

        public TrafficViewModel Sample { get; set; }
    }
}
