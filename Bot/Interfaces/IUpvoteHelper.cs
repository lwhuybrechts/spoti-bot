namespace Spoti_bot.Bot.Interfaces
{
    public interface IUpvoteHelper
    {
        bool EndsWithUpvote(string text);
        string AddUpvote(string text);
        string IncrementUpvote(string text);
    }
}
