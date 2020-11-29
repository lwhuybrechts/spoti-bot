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

        private static Regex GetRegex(InlineQueryCommand inlineQuery)
        {
            // We support inline queries as a command with one query attached.
            return new Regex($"^({inlineQuery.ToDescriptionString()} )([\\w\\d]+)");
        }
    }
}