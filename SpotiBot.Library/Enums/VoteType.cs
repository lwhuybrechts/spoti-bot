using static SpotiBot.Library.Enums.VoteAttributes;

namespace SpotiBot.Library.Enums
{
    public enum VoteType
    {
        Upvote,
        [UseNegativeOperator]
        Downvote
    }
}
