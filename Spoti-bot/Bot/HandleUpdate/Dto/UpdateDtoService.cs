using AutoMapper;
using Spoti_bot.Bot.Chats;
using Spoti_bot.Bot.Users;
using Spoti_bot.Spotify;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Playlists;
using Spoti_bot.Spotify.Tracks;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot.HandleUpdate.Dto
{
    public class UpdateDtoService : IUpdateDtoService
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IAuthorizationTokenRepository _authorizationTokenRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ITrackRepository _trackRepository;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly IMapper _mapper;

        public UpdateDtoService(
            IUserRepository userRepository,
            IChatRepository chatRepository,
            IAuthorizationTokenRepository authorizationTokenRepository,
            IPlaylistRepository playlistRepository,
            ITrackRepository trackRepository,
            ISpotifyLinkHelper spotifyLinkHelper,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _authorizationTokenRepository = authorizationTokenRepository;
            _playlistRepository = playlistRepository;
            _trackRepository = trackRepository;
            _spotifyLinkHelper = spotifyLinkHelper;
            _mapper = mapper;
        }

        /// <summary>
        /// Gather all data needed to handle the Update.
        /// </summary>
        public async Task<UpdateDto> Build(Telegram.Bot.Types.Update update)
        {
            if (update == null)
                return null;

            var parsedChat = GetParsedChat(update);
            var parsedUser = GetParsedUser(update);
            var parsedTrackId = await GetParsedTrackId(update);
            var parsedMessage = GetParsedMessage(update);

            var chat = await GetChat(parsedChat?.Id);

            return new UpdateDto
            {
                ParsedUpdateId = GetParsedUpdateId(update),
                ParsedUpdateType = GetParsedUpdateType(update),
                ParsedBotMessageId = GetParsedBotMessageId(update),
                ParsedChat = parsedChat,
                ParsedUser = parsedUser,
                ParsedTrackId = parsedTrackId,
                ParsedTextMessage = GetParsedTextMessage(update),
                ParsedTextMessageWithLinks = GetParsedTextMessageWithLinks(parsedMessage),
                ParsedData = GetParsedData(update),
                ParsedInlineKeyboard = GetInlineKeyboard(parsedMessage),
                Chat = chat,
                User = await GetUser(parsedUser?.Id),
                AuthorizationToken = await GetAuthorizationToken(parsedUser?.Id),
                Playlist = await GetPlaylist(chat?.PlaylistId),
                Track = await GetTrack(parsedTrackId, chat?.PlaylistId)
            };
        }

        private UpdateType? GetParsedUpdateType(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => UpdateType.Message,
                Telegram.Bot.Types.Enums.UpdateType.InlineQuery => UpdateType.InlineQuery,
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => UpdateType.CallbackQuery,
                _ => null,
            };
        }

        /// <summary>
        /// Get the id of this update, can be used to edit the original.
        /// </summary>
        private string GetParsedUpdateId(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => update.Message?.MessageId.ToString(),
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => update.CallbackQuery?.Id,
                Telegram.Bot.Types.Enums.UpdateType.InlineQuery => update.InlineQuery?.Id,
                _ => null
            };
        }

        /// <summary>
        /// Get the id of the message the bot sends after a message with a trackId.
        /// </summary>
        private int? GetParsedBotMessageId(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => update.CallbackQuery.Message.MessageId,
                _ => null 
            };
        }

        /// <summary>
        /// Parse the Chat the Update was sent in.
        /// </summary>
        private Chats.Chat GetParsedChat(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => _mapper.Map<Chats.Chat>(update.Message?.Chat),
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => _mapper.Map<Chats.Chat>(update.CallbackQuery?.Message?.Chat),
                _ => null
            };
        }

        /// <summary>
        /// Parse the User that sent the Update.
        /// </summary>
        private Users.User GetParsedUser(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => _mapper.Map<Users.User>(update.Message?.From),
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => _mapper.Map<Users.User>(update.CallbackQuery?.From),
                Telegram.Bot.Types.Enums.UpdateType.InlineQuery => _mapper.Map<Users.User>(update.InlineQuery?.From),
                _ => null
            };
        }

        /// <summary>
        /// Parse the Spotify TrackId from the message.
        /// </summary>
        private Task<string> GetParsedTrackId(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => _spotifyLinkHelper.ParseTrackId(update.Message?.Text),
                // Callback query is triggered from a message that is a reply to the message with the track url.
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => _spotifyLinkHelper.ParseTrackId(update.CallbackQuery?.Message?.ReplyToMessage?.Text),
                _ => Task.FromResult<string>(null)
            };
        }

        private string GetParsedTextMessage(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => update.Message?.Text,
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => update.CallbackQuery?.Message?.Text,
                Telegram.Bot.Types.Enums.UpdateType.InlineQuery => update.InlineQuery?.Query,
                _ => null
            };
        }

        private string GetParsedTextMessageWithLinks(Message message)
        {
            if (message == null)
                return null;

            return GetTextMessageWithTextLinks(message);
        }

        private InlineKeyboardMarkup GetInlineKeyboard(Message message)
        {
            return message?.ReplyMarkup;
        }

        private Message GetParsedMessage(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => update.Message,
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => update.CallbackQuery?.Message,
                _ => null
            };
        }

        private string GetParsedData(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => update.CallbackQuery.Data,
                _ => null
            };
        }

        private Task<Chats.Chat> GetChat(long? chatId)
        {
            if (!chatId.HasValue)
                return Task.FromResult<Chats.Chat>(null);

            return _chatRepository.Get(chatId.Value);
        }

        private Task<Users.User> GetUser(long? userId)
        {
            if (!userId.HasValue)
                return Task.FromResult<Users.User>(null);

            return _userRepository.Get(userId.Value);
        }

        private Task<AuthorizationToken> GetAuthorizationToken(long? userId)
        {
            if (!userId.HasValue)
                return Task.FromResult<AuthorizationToken>(null);

            return _authorizationTokenRepository.Get(userId.Value);
        }

        private Task<Playlist> GetPlaylist(string playlistId)
        {
            if (string.IsNullOrEmpty(playlistId))
                return Task.FromResult<Playlist>(null);

            return _playlistRepository.Get(playlistId);
        }

        private async Task<Track> GetTrack(string trackId, string playlistId)
        {
            if (string.IsNullOrEmpty(trackId) || string.IsNullOrEmpty(playlistId))
                return null;

            return await _trackRepository.Get(trackId, playlistId);
        }

        /// <summary>
        /// Get the original text and re-add the links to it.
        /// We need to do this since the telegram library we use doesn't offer a way to pass the entities.
        /// </summary>
        /// <param name="textMessage">The message we want the text from.</param>
        /// <returns>The text with all links added to it.</returns>
        private string GetTextMessageWithTextLinks(Message textMessage)
        {
            var text = textMessage.Text;

            if (textMessage.Entities == null)
                return text;

            foreach (var textLinkEntity in textMessage.Entities.Where(x => x.Type == MessageEntityType.TextLink))
            {
                var firstPart = text.Substring(0, textLinkEntity.Offset);
                var linkText = text.Substring(firstPart.Length, textLinkEntity.Length);
                var lastPart = text.Substring(firstPart.Length + linkText.Length);

                text = $"{firstPart}[{linkText}]({textLinkEntity.Url}){lastPart}";
            }

            return text;
        }
    }
}
