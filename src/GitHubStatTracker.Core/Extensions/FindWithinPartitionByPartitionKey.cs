using GitHubStatTracker.Core.Models;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitHubStatTracker.Core.Extensions
{
    public interface IQuery<in TModel, out TResult>
    {
        TResult Execute(TModel model);
    }
    public class FindWithinPartitionStartsWithByPartitionKey
        : IQuery<CloudTable, List<RepoStatEntity>>
    {
        private readonly string startsWithPattern;

        public FindWithinPartitionStartsWithByPartitionKey(//string partitionKey,
            string startsWithPattern)
        {
            if (startsWithPattern == null)
                throw new ArgumentNullException("startsWithPattern");


            this.startsWithPattern = startsWithPattern;
        }

        public List<RepoStatEntity> Execute(CloudTable model)
        {
            var query = new TableQuery<RepoStatEntity>();

            var length = startsWithPattern.Length - 1;
            var lastChar = startsWithPattern[length];

            var nextLastChar = (char)(lastChar + 1);

            var startsWithEndPattern = startsWithPattern.Substring(0, length) + nextLastChar;

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    startsWithPattern),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.LessThan,
                    startsWithEndPattern)
                );

            var entities = model.ExecuteQuery<RepoStatEntity>(query.Where(prefixCondition));

            return entities.ToList();
        }
    }
}
