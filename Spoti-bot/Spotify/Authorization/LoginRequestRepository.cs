using Azure.Data.Tables;
using SpotiBot.Library;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Spotify.Authorization
{
    public class LoginRequestRepository : BaseRepository<LoginRequest>, ILoginRequestRepository
    {
        public LoginRequestRepository(TableServiceClient tableServiceClient)
            : base(tableServiceClient.GetTableClient(typeof(LoginRequest).Name), "loginrequests")
        {
        }

        public new Task<LoginRequest> Get(string rowKey, string partitionKey = "")
        {
            // Make sure the PartitionKey is set.
            if (string.IsNullOrEmpty(partitionKey))
                partitionKey = _defaultPartitionKey;

            // Don't return expired LoginRequests.
            return GetSingle(x => x.PartitionKey == partitionKey && x.RowKey == rowKey && x.ExpiresAt > DateTimeOffset.UtcNow);
        }

        public Task<List<LoginRequest>> GetAllExpired()
        {
            return QueryPageable(x => x.ExpiresAt <= DateTimeOffset.UtcNow);
        }
    }
}
