using static Spoti_bot.Bot.Votes.VoteAttributes;

namespace Spoti_bot.Bot.Votes
{
    public enum VoteType
    {
        Upvote,
        [UseNegativeOperator]
        Downvote
    }
}
