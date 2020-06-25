using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubStatTracker.Core.Models
{
   public class ConfigModel
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AzureWebJobsStorage { get; set; }
    }
}
