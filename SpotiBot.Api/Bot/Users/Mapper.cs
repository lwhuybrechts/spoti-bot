using SpotiBot.Library.BusinessModels.Bot;
using Telegram.Bot.Types.InlineQueryResults;

namespace SpotiBot.Api.Bot.Users
{
    public class Mapper : IMapper
    {
        public ParsedUser Map(Telegram.Bot.Types.User source) => new(
            source.Id,
            source.FirstName,
            source.LastName,
            source.Username,
            source.LanguageCode
        );

        public User Map(ParsedUser source) => new(
            source.Id,
            source.FirstName,
            source.LastName,
            source.UserName
        );

        public InlineQueryResultArticle Map(User source) => new(source.Id.ToString(), source.FirstName, new InputTextMessageContent(source.FirstName));
    }
}
