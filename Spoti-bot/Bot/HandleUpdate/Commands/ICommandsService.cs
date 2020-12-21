using System;
using System.Collections.Generic;

namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    public interface ICommandsService
    {
        bool IsAnyCommand<TCommand>(string text) where TCommand : Enum;
        bool IsCommand<TCommand>(string text, TCommand command) where TCommand : Enum;
        bool HasQuery<TCommand>(string text, TCommand command) where TCommand : Enum;
        string GetQuery<TCommand>(string text, TCommand command) where TCommand : Enum;
        List<string> GetQueries<TCommand>(string text, TCommand command) where TCommand : Enum;
    }
}
