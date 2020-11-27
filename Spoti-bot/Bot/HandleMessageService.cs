using Spoti_bot.Bot.Commands;
using Spoti_bot.Bot.Interfaces;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify.Interfaces;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace Spoti_bot.Bot
{
    public class HandleMessageService : IHandleMessageService
    {
        private readonly ICommandsService _commandsService;
        private readonly IAddTrackService _addTrackService;
        private readonly IUserService _userService;
        private readonly ISpotifyLinkHelper _spotifyTextHelper;

        public HandleMessageService(
            ICommandsService commandsService,
            IAddTrackService spotifyAddTrackService,
            IUserService userService,
            ISpotifyLinkHelper spotifyTextHelper)
        {
            _commandsService = commandsService;
            _addTrackService = spotifyAddTrackService;
            _userService = userService;
            _spotifyTextHelper = spotifyTextHelper;
        }

        public async Task<BotResponseCode> TryHandleMessage(Telegram.Bot.Types.Update update)
        {
            // If the bot can't do anything with the update's message, we're done.
            if (!CanHandleMessage(update))
                return BotResponseCode.NoAction;

            // Check if any command should be handled and if so handle it.
            var commandResponseCode = await _commandsService.TryHandleCommand(update.Message);
            if (commandResponseCode != BotResponseCode.NoAction)
                return commandResponseCode;

            // Try to add a spotify track url that was in the message to the playlist.
            var addTrackResponseCode = await _addTrackService.TryAddTrackToPlaylist(update.Message);
            if (addTrackResponseCode != BotResponseCode.NoAction)
            {
                // Save users that added tracks to the playlist.
                await _userService.SaveUser(update.Message.From);

                return addTrackResponseCode;
            }

            // This should never happen.
            throw new MessageNotHandledException();
        }

        /// <summary>
        /// Check if the bot can handle the update's message. 
        /// </summary>
        /// <param name="update">The update to check.</param>
        /// <returns>True if the bot can handle the message.</returns>
        private bool CanHandleMessage(Telegram.Bot.Types.Update update)
        {
            // Check if we have all the data we need.
            if (update == null ||
                // Filter everything but messages.
                update.Type != UpdateType.Message ||
                update.Message == null ||
                // Filter everything but text messages.
                update.Message.Type != MessageType.Text)
                return false;

            if (_commandsService.IsAnyCommand(update.Message))
                return true;

            if (_spotifyTextHelper.HasAnySpotifyLink(update.Message.Text))
                return true;

            return false;
        }
    }
}
