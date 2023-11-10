using SpotiBot.Library.BusinessModels.Bot;
using Telegram.Bot.Types.InlineQueryResults;

namespace SpotiBot.Api.Bot.Users
{
    public class Mapper : IMapper
    {
        public User Map(Telegram.Bot.Types.User source) => new(
            source.Id,
            source.FirstName,
            source.LastName,
            source.Username,
            source.LanguageCode
        );

        public InlineQueryResultArticle Map(User source) => new(source.Id.ToString(), source.FirstName, new InputTextMessageContent(source.FirstName));
    }
}
