using GitHubStatTracker.Core.Extensions;
using GitHubStatTracker.Core.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubStatTracker.Core.Services
{
    public class TableStorageService
    {
        private readonly IConfiguration _configuration;


        public TableStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }

            return storageAccount;
        }

        public async Task<CloudTable> CreateTableAsync(string tableName)
        {
            string storageConnectionString = _configuration.GetValue<string>("AzureWebJobsStorage");

            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
            }
            else
            {
            }
            return table;
        }

        public async Task<RepoStatEntity> InsertOrMergeEntityAsync(RepoStatEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            try
            {
                var table = await CreateTableAsync("GitHubRepoStats");

                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                RepoStatEntity insertedStat = result.Result as RepoStatEntity;

                return insertedStat;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }


        public async Task<List<RepoStatEntity>> GetActiveRepos()
        {
            var table = await CreateTableAsync("GitHubRepoStats");

            var query = new TableQuery<RepoStatEntity>();

            var condition = TableQuery.GenerateFilterConditionForBool("SyncEnabled", QueryComparisons.Equal, true);
            var results = table.ExecuteQuery<RepoStatEntity>(query.Where(condition)).ToList();

            var ordered = results.GroupBy(grp => grp.PartitionKey)
      .Select(g => g.OrderByDescending(grp => DateTime.Parse(grp.Date)).FirstOrDefault()).ToList();

            return ordered;
        }


        public async Task<List<RepoStatEntity>> GetDataForUser(string user, CancellationToken ct)
        {
            var table = await CreateTableAsync("GitHubRepoStats");

            var query = new FindWithinPartitionStartsWithByPartitionKey(user);
            var results = query.Execute(table);

            return results.ToList();
        }

        public async Task<List<RepoStatEntity>> GetDataForUserRepo(string userrepo, CancellationToken ct)
        {
            var table = await CreateTableAsync("GitHubRepoStats");
            var query = new TableQuery<RepoStatEntity>();

            var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userrepo);

            var results = table.ExecuteQuery<RepoStatEntity>(query.Where(condition));

            return results.ToList();
        }

        public async Task<RepoStatEntity> RetrieveEntityUsingPointQueryAsync(CloudTable table, string partitionKey, string rowKey)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<RepoStatEntity>(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(retrieveOperation);
                RepoStatEntity stat = result.Result as RepoStatEntity;
                return stat;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
