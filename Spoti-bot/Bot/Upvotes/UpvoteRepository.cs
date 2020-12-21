using Microsoft.Azure.Cosmos.Table;
using Spoti_bot.Library;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Upvotes
{
    public class UpvoteRepository : BaseRepository<Upvote>, IUpvoteRepository
    {
        public UpvoteRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Upvote).Name), null)
        {
            
        }

        public Task<List<Upvote>> GetUpvotes(long userId)
        {
            return GetAllByRowKey(userId.ToString());
        }
    }
}
