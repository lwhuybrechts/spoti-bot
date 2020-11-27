using Microsoft.Extensions.Options;
using Spoti_bot.Bot.Interfaces;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot.Commands
{
    public class CommandsService : ICommandsService
    {
        private readonly ISpotifyAuthorizationService _spotifyAuthorizationService;
        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly ISyncTracksService _syncTracksService;
        private readonly TelegramOptions _telegramOptions;

        public CommandsService(
            ISpotifyAuthorizationService spotifyAuthorizationService,
            ISendMessageService sendMessageService,
            ISpotifyLinkHelper spotifyLinkHelper,
            ISyncTracksService trackService,
            IOptions<TelegramOptions> telegramOptions)
        {
            _spotifyAuthorizationService = spotifyAuthorizationService;
            _sendMessageService = sendMessageService;
            _spotifyLinkHelper = spotifyLinkHelper;
            _syncTracksService = trackService;
            _telegramOptions = telegramOptions.Value;
        }

        /// <summary>
        /// Check if the message is extactly the same as any of the defined commands.
        /// </summary>
        /// <param name="message">The message to check for.</param>
        /// <returns>True if the message is one of the defined commands.</returns>
        public bool IsAnyCommand(Message message)
        {
            if (message == null || string.IsNullOrEmpty(message.Text))
                return false;

            foreach (var command in Enum.GetValues(typeof(Command)).Cast<Command>())
                if (IsCommand(message, command))
                    return true;

            return false;
        }

        /// <summary>
        /// Check if a message contains a command and if so, handle it.
        /// </summary>
        /// <param name="message">The message to check for commands.</param>
        /// <returns>True is a command was handled, false if no matching command was found.</returns>
        public async Task<BotResponseCode> TryHandleCommand(Message message)
        {
            if (IsCommand(message, Command.Test))
            {
                await _sendMessageService.SendTextMessageAsync(message.Chat.Id, GetRandomTestCommandResponse());
                return BotResponseCode.TestCommandHandled;
            }

            if (IsCommand(message, Command.Help))
            {
                var helpText = $"Welcome to Spoti-bot.\n\n" +
                    $"Post links to Spotify tracks in this chat and they will be added to the playlist {_spotifyLinkHelper.GetMarkdownLinkToPlaylist()}.";
                
                await _sendMessageService.SendTextMessageAsync(message.Chat.Id, helpText, disableWebPagePreview: false);
                return BotResponseCode.HelpCommandHandled;
            }

            if (IsCommand(message, Command.GetLoginLink))
            {
                Uri loginUri = _spotifyAuthorizationService.GetLoginUri();
                var keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Login to Spotify", loginUri.ToString()));

                var loginText = $"Please login to authorize Spoti-Bot.\n" +
                    $"It needs access to the playlist {_spotifyLinkHelper.GetMarkdownLinkToPlaylist()} and to your queue.";

                await _sendMessageService.SendTextMessageAsync(message.Chat.Id, loginText, replyMarkup: keyboard);
                return BotResponseCode.GetLoginLinkCommandHandled;
            }

            if (IsCommand(message, Command.ResetPlaylistStorage))
            {
                await _syncTracksService.SyncTracks();
                await _sendMessageService.SendTextMessageAsync(message.Chat.Id, "Spoti-bot playlist storage has been synced.");
                return BotResponseCode.ResetCommandHandled;
            }

            return BotResponseCode.NoAction;
        }

        /// <summary>
        /// Check if the message is extactly the same as one of the defined commands.
        /// </summary>
        /// <param name="message">The message to check for.</param>
        /// <param name="command">The command we're checking.</param>
        /// <returns>True if the message is the command.</returns>
        private bool IsCommand(Message message, Command command)
        {
            return message.Text == command.ToDescriptionString()
                // TODO: get bot username from telegram api.
                || message.Text == $"{command.ToDescriptionString()}@{_telegramOptions.BotUserName}";
        }

        /// <summary>
        /// The test command can be used to check if the bot is running.
        /// </summary>
        /// <returns>A silly response to let us know the bot is running.</returns>
        private string GetRandomTestCommandResponse()
        {
            var responses = new[]
            {
                $"Hi {Command.Test.ToDescriptionString()}, how's it going?",
                "Beep boop, I'm a bot.",
                "Spoti-bot is live and ready."
            };
            var randomIndex = new Random().Next(responses.Length);

            return responses[randomIndex];
        }
    }
}
