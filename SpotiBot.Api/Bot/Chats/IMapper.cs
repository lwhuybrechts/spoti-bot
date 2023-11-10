namespace SpotiBot.Api.Bot.Chats
{
    public interface IMapper
    {
        ParsedChat Map(Telegram.Bot.Types.Chat source);
    }
}
