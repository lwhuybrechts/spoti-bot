using SpotiBot.Library.BusinessModels.Bot;
using Telegram.Bot.Types.InlineQueryResults;

namespace SpotiBot.Api.Bot.Users
{
    public interface IMapper
    {
        User Map(Telegram.Bot.Types.User source);
        InlineQueryResultArticle Map(User source);
    }
}
