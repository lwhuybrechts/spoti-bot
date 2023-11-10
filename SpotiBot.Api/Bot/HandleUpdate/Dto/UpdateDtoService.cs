using SpotiBot.Api.Bot.Chats;
using SpotiBot.Api.Bot.Users;
using SpotiBot.Api.Spotify;
using SpotiBot.Data.Services;
using SpotiBot.Library.BusinessModels.Spotify;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using IUserService = SpotiBot.Data.Services.IUserService;

namespace SpotiBot.Api.Bot.HandleUpdate.Dto
{
    public class UpdateDtoService : IUpdateDtoService
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly IAuthorizationTokenService _authorizationTokenService;
        private readonly IPlaylistService _playlistService;
        private readonly ITrackService _trackService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly Chats.IMapper _chatsMapper;
        private readonly Users.IMapper _usersMapper;

        public UpdateDtoService(
            IUserService userService,
            IChatService chatService,
            IAuthorizationTokenService authorizationTokenService,
            IPlaylistService playlistService,
            ITrackService trackService,
            ISpotifyLinkHelper spotifyLinkHelper,
            Chats.IMapper chatsMapper,
            Users.IMapper usersMapper)
        {
            _userService = userService;
            _chatService = chatService;
            _authorizationTokenService = authorizationTokenService;
            _playlistService = playlistService;
            _trackService = trackService;
            _spotifyLinkHelper = spotifyLinkHelper;
            _chatsMapper = chatsMapper;
            _usersMapper = usersMapper;
        }

        /// <summary>
        /// Gather all data needed to handle the Update.
        /// </summary>
        public async Task<UpdateDto?> Build(Telegram.Bot.Types.Update? update)
        {
            if (update == null)
                return null;

            var parsedChat = GetParsedChat(update);
            var parsedUser = GetParsedUser(update);
            var parsedTrackId = await GetParsedTrackId(update);
            var parsedMessage = GetParsedMessage(update);

            var chat = await GetChat(parsedChat?.Id);

            return new UpdateDto(
                GetParsedUpdateId(update),
                GetParsedUpdateType(update),
                GetParsedBotMessageId(update),
                parsedChat,
                parsedUser,
                parsedTrackId,
                GetParsedTextMessage(update),
                GetParsedTextMessageWithLinks(parsedMessage),
                GetInlineKeyboard(parsedMessage),
                GetParsedData(update),
                chat,
                await GetUser(parsedUser?.Id),
                await GetAuthorizationToken(parsedUser?.Id),
                await GetPlaylist(chat?.PlaylistId),
                await GetTrack(parsedTrackId, chat?.PlaylistId)
            );
        }

        private static UpdateType? GetParsedUpdateType(Telegram.Bot.Types.Update update)
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
        private static string GetParsedUpdateId(Telegram.Bot.Types.Update update)
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
        private static int? GetParsedBotMessageId(Telegram.Bot.Types.Update update)
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
        private ParsedChat? GetParsedChat(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => _chatsMapper.Map(update.Message?.Chat),
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => _chatsMapper.Map(update.CallbackQuery?.Message?.Chat),
                _ => null
            };
        }

        /// <summary>
        /// Parse the User that sent the Update.
        /// </summary>
        private ParsedUser GetParsedUser(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => _usersMapper.Map(update.Message?.From),
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => _usersMapper.Map(update.CallbackQuery?.From),
                Telegram.Bot.Types.Enums.UpdateType.InlineQuery => _usersMapper.Map(update.InlineQuery?.From),
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

        private static string GetParsedTextMessage(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => update.Message?.Text,
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => update.CallbackQuery?.Message?.Text,
                Telegram.Bot.Types.Enums.UpdateType.InlineQuery => update.InlineQuery?.Query,
                _ => null
            };
        }

        private static string GetParsedTextMessageWithLinks(Message message)
        {
            if (message == null)
                return null;

            return GetTextMessageWithTextLinks(message);
        }

        private static InlineKeyboardMarkup GetInlineKeyboard(Message message)
        {
            return message?.ReplyMarkup;
        }

        private static Message GetParsedMessage(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => update.Message,
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => update.CallbackQuery?.Message,
                _ => null
            };
        }

        private static string GetParsedData(Telegram.Bot.Types.Update update)
        {
            return update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => update.CallbackQuery.Data,
                _ => null
            };
        }

        private Task<SpotiBot.Library.BusinessModels.Bot.Chat> GetChat(long? chatId)
        {
            if (!chatId.HasValue)
                return Task.FromResult<SpotiBot.Library.BusinessModels.Bot.Chat>(null);

            return _chatService.Get(chatId.Value);
        }

        private Task<SpotiBot.Library.BusinessModels.Bot.User> GetUser(long? userId)
        {
            if (!userId.HasValue)
                return Task.FromResult<SpotiBot.Library.BusinessModels.Bot.User>(null);

            return _userService.Get(userId.Value);
        }

        private Task<AuthorizationToken> GetAuthorizationToken(long? userId)
        {
            if (!userId.HasValue)
                return Task.FromResult<AuthorizationToken>(null);

            return _authorizationTokenService.Get(userId.Value);
        }

        private Task<Playlist> GetPlaylist(string playlistId)
        {
            if (string.IsNullOrEmpty(playlistId))
                return Task.FromResult<Playlist>(null);

            return _playlistService.Get(playlistId);
        }

        private async Task<Track> GetTrack(string trackId, string playlistId)
        {
            if (string.IsNullOrEmpty(trackId) || string.IsNullOrEmpty(playlistId))
                return null;

            return await _trackService.Get(trackId, playlistId);
        }

        /// <summary>
        /// Get the original text and re-add the links to it.
        /// We need to do this since the telegram library we use doesn't offer a way to pass the entities.
        /// </summary>
        /// <param name="textMessage">The message we want the text from.</param>
        /// <returns>The text with all links added to it.</returns>
        private static string GetTextMessageWithTextLinks(Message textMessage)
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
