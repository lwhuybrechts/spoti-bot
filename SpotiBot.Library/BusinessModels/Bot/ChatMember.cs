namespace SpotiBot.Library.BusinessModels.Bot
{
    public class ChatMember
    {
        public long ChatId { get; set; }
        public long UserId { get; set; }

        public ChatMember(long chatId, long userId)
        {
            ChatId = chatId;
            UserId = userId;
        }
    }
}
