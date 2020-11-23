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
        private readonly IAddTrackService _spotifyAddTrackService;
        private readonly IUserService _userService;
        private readonly ISpotifyLinkHelper _spotifyTextHelper;

        public HandleMessageService(
            ICommandsService commandsService,
            IAddTrackService spotifyAddTrackService,
            IUserService userService,
            ISpotifyLinkHelper spotifyTextHelper)
        {
            _commandsService = commandsService;
            _spotifyAddTrackService = spotifyAddTrackService;
            _userService = userService;
            _spotifyTextHelper = spotifyTextHelper;
        }

        public async Task<bool> TryHandleMessage(Telegram.Bot.Types.Update update)
        {
            // If the bot can't do anything with the update's message, we're done.
            if (!CanHandleMessage(update))
                return false;

            // Check if any command should be handled and if so handle it.
            if (await _commandsService.TryHandleCommand(update.Message))
                return true;

            // Try to add a spotify track url that was in the message to the playlist.
            if (await _spotifyAddTrackService.TryAddTrackToPlaylist(update.Message))
            {
                // Save users that added tracks to the playlist.
                await _userService.SaveUser(update.Message);

                return true;
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
