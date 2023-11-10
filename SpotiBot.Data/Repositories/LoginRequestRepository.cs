using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Data.Repositories
{
    public class LoginRequestRepository : BaseRepository<LoginRequest>, ILoginRequestRepository
    {
        public LoginRequestRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(LoginRequest).Name), "loginrequests")
        {
        }

        public new Task<LoginRequest?> Get(string rowKey, string partitionKey = "")
        {
            // Make sure the PartitionKey is set.
            if (string.IsNullOrEmpty(partitionKey))
                partitionKey = _defaultPartitionKey;

            var paritionKeyFilter = TableQuery.GenerateFilterCondition(nameof(LoginRequest.PartitionKey), QueryComparisons.Equal, partitionKey);
            var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(LoginRequest.RowKey), QueryComparisons.Equal, rowKey);

            // Don't return expired LoginRequests.
            var expiresAtFilter = TableQuery.GenerateFilterConditionForDate(nameof(LoginRequest.ExpiresAt), QueryComparisons.GreaterThan, DateTimeOffset.UtcNow);

            var filter = TableQuery.CombineFilters(TableQuery.CombineFilters(paritionKeyFilter, TableOperators.And, rowKeyFilter), TableOperators.And, expiresAtFilter);

            var query = new TableQuery<LoginRequest>().Where(filter);

            return GetSingle(query);
        }

        public Task<List<LoginRequest>> GetAllExpired()
        {
            var expiresAtFilter = TableQuery.GenerateFilterConditionForDate(nameof(LoginRequest.ExpiresAt), QueryComparisons.LessThanOrEqual, DateTimeOffset.UtcNow);
            var query = new TableQuery<LoginRequest>().Where(expiresAtFilter);

            return ExecuteSegmentedQueries(query);
        }
    }
}
