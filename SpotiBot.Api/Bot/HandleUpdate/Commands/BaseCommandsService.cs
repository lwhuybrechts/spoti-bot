using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using SpotiBot.Api.Library.Exceptions;
using SpotiBot.Api.Library.Extensions;
using SpotiBot.Api.Spotify;
using SpotiBot.Library.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpotiBot.Api.Bot.HandleUpdate.Commands
{
    public abstract class BaseCommandsService<TCommand> : IBaseCommandsService where TCommand : Enum
    {
        private readonly ICommandsService _commandsService;
        private readonly Users.IUserService _userService;
        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;

        public BaseCommandsService(
            ICommandsService commandsService,
            Users.IUserService userService,
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
                    var errorText = await ValidateRequirements(command, updateDto);

                    if (!string.IsNullOrEmpty(errorText))
                    {
                        // If there is a chat, answer with the errorText.
                        if (updateDto.ParsedChat != null)
                            await _sendMessageService.SendTextMessage(updateDto.ParsedChat.Id, errorText);

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
            if (command.HasAttribute<TCommand, RequiresPrivateChatAttribute>() &&
                updateDto.ParsedChat?.Type != ChatType.Private)
                return $"The {command} command is only supported in private chats.";

            if (command.HasAttribute<TCommand, RequiresChatAttribute>() && updateDto.Chat == null)
                return $"Spoti-bot first needs to be added to this chat by sending the {Command.Start.ToDescriptionString()} command.";

            if (command.HasAttribute<TCommand, RequiresNoChatAttribute>() && updateDto.Chat != null)
            {
                var admin = await _userService.GetAdmin(updateDto);

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

                var admin = await _userService.GetAdmin(updateDto);

                // A chat should always have an admin.
                if (admin == null)
                    throw new ChatAdminNullException(updateDto.Chat.Id, updateDto.Chat.AdminUserId);

                if (updateDto.User.Id != admin.Id)
                    return $"Only the chat admin ({admin.FirstName}) can use this command.";
            }

            if (command.HasAttribute<TCommand, RequiresPlaylistAttribute>() && updateDto.Playlist == null)
                return $"Please set a playlist first, with the {Command.SetPlaylist.ToDescriptionString()} command.";

            if (command.HasAttribute<TCommand, RequiresNoPlaylistAttribute>() && updateDto.Playlist != null)
                return $"This chat already has a {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(updateDto.Playlist.Id, "playlist")} set.";

            if (command.HasAttribute<TCommand, RequiresQueryAttribute>() && !_commandsService.HasQuery(updateDto.ParsedTextMessage, command))
            {
                var text = $"Please add a query after the {command.ToDescriptionString()} command.";

                if (command.GetType() == typeof(Command) && (Command)(object)command == Command.SetPlaylist)
                    text += $"\n\nFor example:\n{command.ToDescriptionString()} https://open.spotify.com/playlist/37i9dQZEVXbMDoHDwVN2tF";

                return text;
            }

            return string.Empty;
        }

        protected abstract Task<BotResponseCode> HandleCommand(TCommand command, UpdateDto updateDto);
    }
}
