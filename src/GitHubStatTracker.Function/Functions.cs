using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GitHubStatTracker.Core.Models;
using GitHubStatTracker.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubStatTracker.Function
{
    public class Functions
    {
        private readonly GHSTTrackRepos ghst;
        private readonly PBITrackRepos pbi;
        private readonly BlobStorageService blobService;

        public Functions(GHSTTrackRepos _ghst, PBITrackRepos _pbi, BlobStorageService _blobService)
        {
            pbi = _pbi;
            blobService = _blobService;
            ghst = _ghst;
        }

        [FunctionName("GHSTTrackReposTimer")]
        public async void GHSTTrackReposTimer([TimerTrigger("0 0 8 * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
           log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
           await ghst.Run();
        }

        [FunctionName("GHSTTrackReposHttp")]
        public async Task<IActionResult> GHSTTrackReposHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            await ghst.Run();

            return new OkObjectResult("");
        }

        [FunctionName("PBITrackReposTimer")]
        public async void PBITrackReposTimer([TimerTrigger("0 0 8 * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
           log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
           await pbi.Run();
        }

        [FunctionName("PBITrackReposHttp")]
        public async Task<IActionResult> PBITrackReposHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            await pbi.Run();

            return new OkObjectResult("");
        }


        [FunctionName("GetRepos")]
        public async Task<HttpResponseMessage> GetRepos([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ExecutionContext context, ILogger log)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(await blobService.GetRepos(), Encoding.UTF8, "application/json")
            };
        }

        [FunctionName("UpdateRepos")]
        public async Task<IActionResult> UpdateRepos([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            List<Campaign> data = JsonConvert.DeserializeObject<List<Campaign>>(requestBody);

            await blobService.UpdateRepos(data);
            return new OkObjectResult("");
        }

            // returns a single page application to build links
            [FunctionName("Manager")]
        public static HttpResponseMessage Admin([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
            ILogger log)
        {
            const string PATH = "RepoManager.html";

            var result = SecurityCheck(req);
            if (result != null)
            {
                return result;
            }

            var scriptPath = Path.Combine(Environment.CurrentDirectory, "www");
            if (!Directory.Exists(scriptPath))
            {
                scriptPath = Path.Combine(
                    Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process),
                    @"site\wwwroot\www");
            }
            var filePath = Path.GetFullPath(Path.Combine(scriptPath, PATH));

            if (!File.Exists(filePath))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            log.LogInformation($"Attempting to retrieve file at path {filePath}.");
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(filePath, System.IO.FileMode.Open);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        private static HttpResponseMessage SecurityCheck(HttpRequestMessage req)
        {
            return req.RequestUri.IsLoopback || req.RequestUri.Scheme == "https" ? null :
                req.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}