using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    public abstract class BaseCommandsService<TCommand> : IBaseCommandsService where TCommand : Enum
    {
        private readonly ICommandsService _commandsService;
        private readonly IUserService _userService;
        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;

        public BaseCommandsService(
            ICommandsService commandsService,
            IUserService userService,
            ISendMessageService sendMessageService,
            ISpotifyLinkHelper spotifyLinkHelper)
        {
            _commandsService = commandsService;
            _userService = userService;
            _sendMessageService = sendMessageService;
            _spotifyLinkHelper = spotifyLinkHelper;
        }

        /// <summary>
        /// Check if the message contains a command.
        /// </summary>
        public bool IsAnyCommand(string text)
        {
            return _commandsService.IsAnyCommand<TCommand>(text);
        }

        /// <summary>
        /// Check if a message contains a command and if so, handle it.
        /// </summary>
        /// <returns>A BotResponseCode if a command was handled, or NoAction if no matching command was found.</returns>
        public async Task<BotResponseCode> TryHandleCommand(UpdateDto updateDto)
        {
            foreach (var command in Enum.GetValues(typeof(TCommand)).Cast<TCommand>())
                if (_commandsService.IsCommand(updateDto.ParsedTextMessage, command))
                {
                    var text = await ValidateRequirements(command, updateDto);

                    if (!string.IsNullOrEmpty(text))
                    {
                        // If there is a chat, answer that the requirement is not fulfilled.
                        if (updateDto.ParsedChat != null)
                            await _sendMessageService.SendTextMessage(updateDto.ParsedChat.Id, text);
                        
                        return BotResponseCode.CommandRequirementNotFulfilled;
                    }

                    var responseCode = await HandleCommand(command, updateDto);
                    if (responseCode != BotResponseCode.NoAction)
                        return responseCode;
                }

            return BotResponseCode.NoAction;
        }

        /// <summary>
        /// Validates if all the commands requirements were met.
        /// </summary>
        /// <returns>An empty string if all requirements were met, or an error message.</returns>
        private async Task<string> ValidateRequirements(TCommand command, UpdateDto updateDto)
        {
            // TODO: replace with fluent validation.
            if (command.HasAttribute<TCommand, RequiresChatAttribute>() && updateDto.Chat == null)
                return $"Spoti-bot first needs to be added to this chat by sending the {Command.Start.ToDescriptionString()} command.";

            if (command.HasAttribute<TCommand, RequiresNoChatAttribute>() && updateDto.Chat != null)
            {
                var admin = await _userService.Get(updateDto.Chat.AdminUserId);

                // A chat should always have an admin.
                if (admin == null)
                    throw new ChatAdminNullException(updateDto.Chat.Id, updateDto.Chat.AdminUserId);

                if (updateDto.User.Id != admin.Id)
                    return $"Spoti-bot is already added to this chat, {admin.FirstName} is it's admin.";
                else
                    return $"Spoti-bot is already added to this chat, you are it's admin.";
            }

            if (command.HasAttribute<TCommand, RequiresChatAdminAttribute>())
            {
                if (updateDto.Chat == null)
                    return $"Spoti-bot first needs to be added to this chat by sending the {Command.Start.ToDescriptionString()} command.";

                var admin = await _userService.Get(updateDto.Chat.AdminUserId);

                // A chat should always have an admin.
                if (admin == null)
                    throw new ChatAdminNullException(updateDto.Chat.Id, updateDto.Chat.AdminUserId);

                if (updateDto.User.Id != admin.Id)
                    return $"Only the chat admin ({admin.FirstName}) can use this command.";
            }

            if (command.HasAttribute<TCommand, RequiresPlaylistAttribute>() && updateDto.Playlist == null)
                return $"Please set a playlist first, with command {Command.SetPlaylist.ToDescriptionString()}.";

            if (command.HasAttribute<TCommand, RequiresNoPlaylistAttribute>() && updateDto.Playlist != null)
                return $"This chat already has a { _spotifyLinkHelper.GetMarkdownLinkToPlaylist(updateDto.Playlist.Id, "playlist")} set.";

            if (command.HasAttribute<TCommand, RequiresQueryAttribute>() && !_commandsService.HasQuery(updateDto.ParsedTextMessage, command))
                return $"Please add a query after the {command.ToDescriptionString()} command.";

            return string.Empty;
        }

        protected abstract Task<BotResponseCode> HandleCommand(TCommand command, UpdateDto updateDto);
    }
}
