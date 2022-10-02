using SpotiBot.Bot.HandleUpdate.Commands;
using SpotiBot.Bot.HandleUpdate.Dto;
using SpotiBot.Bot.Users;
using SpotiBot.Library;
using SpotiBot.Library.Exceptions;
using SpotiBot.Spotify;
using SpotiBot.Spotify.Tracks.AddTrack;
using System.Threading.Tasks;

namespace SpotiBot.Bot.HandleUpdate
{
    public class HandleMessageService : IHandleMessageService
    {
        private readonly IHandleCommandService _handleCommandService;
        private readonly IAddTrackService _addTrackService;
        private readonly IUserService _userService;
        private readonly ISpotifyLinkHelper _spotifyTextHelper;

        public HandleMessageService(
            IHandleCommandService handleCommandService,
            IAddTrackService addTrackService,
            IUserService userService,
            ISpotifyLinkHelper spotifyLinkHelper)
        {
            _handleCommandService = handleCommandService;
            _addTrackService = addTrackService;
            _userService = userService;
            _spotifyTextHelper = spotifyLinkHelper;
        }

        public async Task<BotResponseCode> TryHandleMessage(UpdateDto updateDto)
        {
            // If the bot can't do anything with the update's message, we're done.
            if (!CanHandleMessage(updateDto))
                return BotResponseCode.NoAction;

            // Check if any command should be handled and if so handle it.
            var commandResponseCode = await _handleCommandService.TryHandleCommand(updateDto);
            if (commandResponseCode != BotResponseCode.NoAction)
                return commandResponseCode;

            // Try to add a spotify track url that was in the message to the playlist.
            var addTrackResponseCode = await _addTrackService.TryAddTrackToPlaylist(updateDto);
            if (addTrackResponseCode != BotResponseCode.NoAction)
            {
                if (addTrackResponseCode == BotResponseCode.TrackAddedToPlaylist)
                    // Save users that added tracks to the playlist.
                    await _userService.SaveUser(updateDto.ParsedUser, updateDto.ParsedChat.Id);

                return addTrackResponseCode;
            }

            // This should never happen.
            throw new MessageNotHandledException();
        }

        /// <summary>
        /// Check if the bot can handle the update's message. 
        /// </summary>
        private bool CanHandleMessage(UpdateDto updateDto)
        {
            // Check if we have all the data we need.
            if (
                // Filter everything but messages.
                updateDto.ParsedUpdateType != UpdateType.Message ||
                string.IsNullOrEmpty(updateDto.ParsedTextMessage))
                return false;

            // Always handle commands.
            if (_handleCommandService.IsAnyCommand(updateDto.ParsedTextMessage))
                return true;

            // If no playlist was set for this chat, we're done.
            if (updateDto.Chat == null || updateDto.Playlist == null)
                return false;

            if (!_spotifyTextHelper.HasAnyTrackLink(updateDto.ParsedTextMessage))
                return false;

            return true;
        }
    }
}
