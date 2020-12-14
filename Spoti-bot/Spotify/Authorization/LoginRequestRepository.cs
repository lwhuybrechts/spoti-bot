using Microsoft.Azure.Cosmos.Table;
using Spoti_bot.Library;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public class LoginRequestRepository : BaseRepository<LoginRequest>, ILoginRequestRepository
    {
        public LoginRequestRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(LoginRequest).Name), "loginrequests")
        {
        }

        public new Task<LoginRequest> Get(string rowKey, string partitionKey = "")
        {
            // Make sure the PartitionKey is set.
            if (string.IsNullOrEmpty(partitionKey))
                partitionKey = _defaultPartitionKey;

            var paritionKeyFilter = TableQuery.GenerateFilterCondition(nameof(LoginRequest.PartitionKey), QueryComparisons.Equal, partitionKey);
            var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(LoginRequest.RowKey), QueryComparisons.Equal, rowKey);
            
            // Don't return expired LoginRequests.
            var expiresAtFilter = TableQuery.GenerateFilterConditionForDate(nameof(LoginRequest.ExpiresAt), QueryComparisons.GreaterThan, DateTimeOffset.UtcNow);
            
            var query = new TableQuery<LoginRequest>().Where(paritionKeyFilter).Where(rowKeyFilter).Where(expiresAtFilter);

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
