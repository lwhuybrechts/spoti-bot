using Spoti_bot.Bot.Chats;
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
        private readonly IChatRepository _chatRepository;

        public HandleMessageService(
            IHandleCommandService handleCommandService,
            IAddTrackService addTrackService,
            IUserService userService,
            ISpotifyLinkHelper spotifyLinkHelper,
            IChatRepository chatRepository)
        {
            _handleCommandService = handleCommandService;
            _addTrackService = addTrackService;
            _userService = userService;
            _spotifyTextHelper = spotifyLinkHelper;
            _chatRepository = chatRepository;
        }

        public async Task<BotResponseCode> TryHandleMessage(Telegram.Bot.Types.Update update)
        {
            var chat = await GetChat(update);

            // If the bot can't do anything with the update's message, we're done.
            if (!CanHandleMessage(update, chat))
                return BotResponseCode.NoAction;

            // Check if any command should be handled and if so handle it.
            var commandResponseCode = await _handleCommandService.TryHandleCommand(update.Message, chat);
            if (commandResponseCode != BotResponseCode.NoAction)
                return commandResponseCode;

            // Try to add a spotify track url that was in the message to the playlist.
            var addTrackResponseCode = await _addTrackService.TryAddTrackToPlaylist(update.Message, chat);
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
        private bool CanHandleMessage(Telegram.Bot.Types.Update update, Chat chat)
        {
            // Check if we have all the data we need.
            if (update == null ||
                // Filter everything but messages.
                update.Type != UpdateType.Message ||
                update.Message == null ||
                // Filter everything but text messages.
                update.Message.Type != MessageType.Text)
                return false;

            // Always handle commands.
            if (_handleCommandService.IsAnyCommand(update.Message))
                return true;

            // If no playlist was set for this chat, we're done.
            if (chat == null || string.IsNullOrEmpty(chat.PlaylistId))
                return false;

            if (!_spotifyTextHelper.HasAnyTrackLink(update.Message.Text))
                return false;

            return true;
        }

        /// <summary>
        /// Parse the chatId from the message and get the chat from storage.
        /// </summary>
        private Task<Chat> GetChat(Telegram.Bot.Types.Update update)
        {
            var chatId = update?.Message?.Chat?.Id;
            if (!chatId.HasValue)
                return Task.FromResult<Chat>(null);

            return _chatRepository.Get(chatId.Value);
        }
    }
}
