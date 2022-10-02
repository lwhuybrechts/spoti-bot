using static SpotiBot.Bot.Votes.VoteAttributes;

namespace SpotiBot.Bot.Votes
{
    public enum VoteType
    {
        Upvote,
        [UseNegativeOperator]
        Downvote
    }
}
