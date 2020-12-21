using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify;
using Spoti_bot.Spotify.Tracks.AddTrack;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace Spoti_bot.Bot.HandleUpdate
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
            if (updateDto.Update == null ||
                // Filter everything but messages.
                updateDto.Update.Type != UpdateType.Message ||
                updateDto.Update.Message == null ||
                // Filter everything but text messages.
                updateDto.Update.Message.Type != MessageType.Text)
                return false;

            // Always handle commands.
            if (_handleCommandService.IsAnyCommand(updateDto.ParsedTextMessage))
                return true;

            // If no playlist was set for this chat, we're done.
            if (updateDto.Chat == null || string.IsNullOrEmpty(updateDto.Chat.PlaylistId))
                return false;

            if (!_spotifyTextHelper.HasAnyTrackLink(updateDto.ParsedTextMessage))
                return false;

            return true;
        }
    }
}
