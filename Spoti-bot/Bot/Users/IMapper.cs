using Telegram.Bot.Types.InlineQueryResults;

namespace Spoti_bot.Bot.Users
{
    public interface IMapper
    {
        User Map(Telegram.Bot.Types.User source);
        InlineQueryResultArticle Map(User source);
    }
}
