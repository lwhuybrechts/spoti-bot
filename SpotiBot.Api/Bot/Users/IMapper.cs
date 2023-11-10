using SpotiBot.Library.BusinessModels.Bot;
using Telegram.Bot.Types.InlineQueryResults;

namespace SpotiBot.Api.Bot.Users
{
    public interface IMapper
    {
        ParsedUser Map(Telegram.Bot.Types.User source);
        User Map(ParsedUser source);
        InlineQueryResultArticle Map(User source);
    }
}
