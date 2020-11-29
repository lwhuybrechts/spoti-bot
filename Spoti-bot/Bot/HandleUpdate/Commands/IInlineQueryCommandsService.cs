namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    public interface IInlineQueryCommandsService
    {
        bool IsAnyCommand(string inlineQuery);
        bool IsCommand(string inlineQuery, InlineQueryCommand command);
        string GetQuery(string inlineQuery, InlineQueryCommand command);
    }
}