namespace Spoti_bot.Bot.Interfaces
{
    public interface IUpvoteTextHelper
    {
        string IncrementUpvote(string text);
        string DecrementUpvote(string text);
    }
}
