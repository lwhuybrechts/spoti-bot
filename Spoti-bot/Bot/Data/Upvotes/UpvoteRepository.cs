using Microsoft.Azure.Cosmos.Table;
using Spoti_bot.Library;

namespace Spoti_bot.Bot.Data.Upvotes
{
    public class UpvoteRepository : BaseRepository<Upvote>, IUpvoteRepository
    {
        public UpvoteRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Upvote).Name), "upvotes")
        {

        }
    }
}
