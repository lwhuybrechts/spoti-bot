namespace SpotiBot.Bot.Chats
{
    public interface IMapper
    {
        Chat Map(Telegram.Bot.Types.Chat source);
    }
}
