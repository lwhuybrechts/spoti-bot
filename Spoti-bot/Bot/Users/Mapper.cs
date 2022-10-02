using Telegram.Bot.Types.InlineQueryResults;

namespace SpotiBot.Bot.Users
{
    public class Mapper : IMapper
    {
        public User Map(Telegram.Bot.Types.User source) => new()
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            UserName = source.Username,
            LanguageCode = source.LanguageCode
        };

        public InlineQueryResultArticle Map(User source) => new(source.Id.ToString(), source.FirstName, new InputTextMessageContent(source.FirstName));
    }
}
