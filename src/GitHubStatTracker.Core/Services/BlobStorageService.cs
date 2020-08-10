using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GitHubStatTracker.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GitHubStatTracker.Core.Services
{
    public class BlobStorageService
    {
        public async Task<string> GetRepos()
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("repos");
            BlobClient blobClient = containerClient.GetBlobClient("Repos.json");
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            return new StreamReader(download.Content).ReadToEnd();          
        }

        public async Task UpdateRepos(List<Campaign> data)
        {
            var requestBody = JsonConvert.SerializeObject(data, Formatting.Indented);

            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("repos");
            BlobClient blobClient = containerClient.GetBlobClient("Repos.json");

            blobClient.DeleteIfExists();

            using (var stream = new MemoryStream(Encoding.Default.GetBytes(requestBody), false))
            {
                await blobClient.UploadAsync(stream);
            }
        }
    }
}
