using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubStatTracker.Core.Models
{
    public class RepoStatEntity : TableEntity
    {
        public RepoStatEntity()
        {
        }

        public RepoStatEntity(string userrepo, string date)
        {
            PartitionKey = userrepo;
            RowKey = date;
        }

        public string RepoName { get; set; }

        public bool SyncEnabled { get; set; }

        public string AccessToken { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }
        public int Views { get; set; }
        public int UniqueUsers { get; set; }
        public int Clones { get; set; }
        public int UniqueClones { get; set; }
        public int RepoId { get; set; }
        public int StarCount { get; set; }
        public int ForksCount { get; set; }

    }
}
