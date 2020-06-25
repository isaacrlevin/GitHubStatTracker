using System;
using System.Threading.Tasks;
using GitHubStatTracker.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace GitHubStatTracker.Function
{
    public  class Functions
    {
        //private readonly TrackRepos _track;

        //public Functions(TrackRepos track)
        //{
        //    _track = track;
        //}

        private readonly TrackRepos t;

        public Functions(TrackRepos _t)
        {
            t = _t;
        }

        [FunctionName("TrackReposTimer")]
        public  async void TrackReposTimer([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)//, TrackRepos _track)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            await t.Run();
        }

        [FunctionName("TrackReposHttp")]
        public  async Task<IActionResult> TrackReposHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ExecutionContext context, ILogger log)//, TrackRepos _track)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            await t.Run();

            return new OkObjectResult("");
        }
    }
}