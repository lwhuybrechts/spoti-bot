namespace Spoti_bot.Bot.Upvotes
{
    public interface IUpvoteTextHelper
    {
        string IncrementUpvote(string text);
        string DecrementUpvote(string text);
    }
}
