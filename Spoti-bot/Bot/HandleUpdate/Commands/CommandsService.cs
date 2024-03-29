﻿using Microsoft.Extensions.Options;
using SpotiBot.Library.Extensions;
using SpotiBot.Library.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpotiBot.Bot.HandleUpdate.Commands
{
    public class CommandsService : ICommandsService
    {
        private readonly TelegramOptions _telegramOptions;
        private const int _maxAmountOfQueries = 3;

        public CommandsService(IOptions<TelegramOptions> telegramOptions)
        {
            _telegramOptions = telegramOptions.Value;
        }

        /// <summary>
        /// Check if the text contains one of the defined commands.
        /// </summary>
        /// <param name="text">The text to check for.</param>
        /// <returns>True if the text contains one of the defined commands.</returns>
        public bool IsAnyCommand<TCommand>(string text) where TCommand : Enum
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (var command in Enum.GetValues(typeof(TCommand)).Cast<TCommand>())
                if (IsCommand(text, command))
                    return true;

            return false;
        }
        
        /// <summary>
        /// Check if the message contains one of the defined commands.
        /// </summary>
        /// <returns>True if the message contains the command.</returns>
        public bool IsCommand<TCommand>(string text, TCommand command) where TCommand : Enum
        {
            return GetRegex(command, true).Match(text).Success ||
                GetRegex(command, false).Match(text).Success;
        }

        /// <summary>
        /// Check if the message contains a command with a query string.
        /// For example: /setplaylist https://open.spotify.com/playlist/3CJH1ko8Dwo2ikJehcOcjM
        /// </summary>
        /// <returns>True if the message contains a command with a query string.</returns>
        public bool HasQuery<TCommand>(string text, TCommand command) where TCommand : Enum
        {
            return GetRegex(command, true).Match(text).Success;
        }

        /// <summary>
        /// Get the query from the command.
        /// </summary>
        public string GetQuery<TCommand>(string text, TCommand command) where TCommand : Enum
        {
            return GetRegex(command, true).Match(text).Groups[1].Value;
        }

        /// <summary>
        /// Get multiple queries from the command.
        /// </summary>
        public List<string> GetQueries<TCommand>(string text, TCommand command) where TCommand : Enum
        {
            return GetRegex(command, true).Match(text).Groups.Values.Select(x => x.Value).ToList();
        }

        private Regex GetRegex<TCommand>(TCommand command, bool addQuery = false) where TCommand : Enum
        {
            var commandString = command.ToDescriptionString();

            // If the command starts with a slash, escape it.
            if (commandString.StartsWith('/'))
                commandString = $"\\{commandString}";

            // TODO: get the bot username from the telegram api.

            // Match the command and optionally the bot username.
            var pattern = $"^(?:{commandString})(?:@{_telegramOptions.BotUserName}|)";

            if (addQuery)
            {
                // The query format is: a command, a space then the query.
                pattern += "\\s+(\\S+)";

                // Add optional queries.
                for (var i = 1; i < _maxAmountOfQueries; i++)
                    pattern += "\\s*(\\S*)";
            }

            pattern += "$";

            return new Regex(pattern);
        }
    }
}