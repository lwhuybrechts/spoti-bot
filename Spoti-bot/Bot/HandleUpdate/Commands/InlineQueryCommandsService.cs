using Spoti_bot.Library;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    public class InlineQueryCommandsService : IInlineQueryCommandsService
    {
        public bool IsAnyCommand(string inlineQuery)
        {
            foreach (var command in Enum.GetValues(typeof(InlineQueryCommand)).Cast<InlineQueryCommand>())
                if (IsCommand(inlineQuery, command))
                    return true;

            return false;
        }

        public bool IsCommand(string inlineQuery, InlineQueryCommand command)
        {
            return GetRegex(command).Match(inlineQuery).Success;
        }

        public string GetQuery(string inlineQuery, InlineQueryCommand command)
        {
            return GetRegex(command).Match(inlineQuery).Groups[2].Value;
        }

        private static Regex GetRegex(InlineQueryCommand command)
        {
            // The inline query format is: a command, a space then the query.
            return new Regex($"^({command.ToDescriptionString()} )([\\w\\d]+)");
        }
    }
}