using AutoMapper;
using Spoti_bot.Bot.Chats;
using Spoti_bot.Bot.Users;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Playlists;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.HandleUpdate.Dto
{
    public class UpdateDtoService : IUpdateDtoService
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IAuthorizationTokenRepository _authorizationTokenRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IMapper _mapper;

        public UpdateDtoService(
            IUserRepository userRepository,
            IChatRepository chatRepository,
            IAuthorizationTokenRepository authorizationTokenRepository,
            IPlaylistRepository playlistRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _authorizationTokenRepository = authorizationTokenRepository;
            _playlistRepository = playlistRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Gather all data needed to handle the Update.
        /// </summary>
        public async Task<UpdateDto> Build(Telegram.Bot.Types.Update update)
        {
            if (update == null)
                return null;

            var parsedUser = GetParsedUser(update);
            var parsedChat = GetParsedChat(update);

            var chat = await GetChat(parsedChat?.Id);

            return new UpdateDto
            {
                Update = update,
                ParsedUser = parsedUser,
                ParsedChat = parsedChat,
                Chat = chat,
                User = await GetUser(parsedUser?.Id),
                AuthorizationToken = await GetAuthorizationToken(parsedUser?.Id),
                Playlist = await GetPlaylist(chat)
            };
        }

        /// <summary>
        /// Parse the Chat the Update was sent in.
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        private Chat GetParsedChat(Telegram.Bot.Types.Update update)
        {
            var chat = update?.Message?.Chat
                ?? update?.CallbackQuery?.Message?.Chat;

            return _mapper.Map<Chat>(chat);
        }

        /// <summary>
        /// Parse the User that sent the Update.
        /// </summary>
        private User GetParsedUser(Telegram.Bot.Types.Update update)
        {
            var user = update?.Message?.From
                ?? update?.CallbackQuery?.From;

            return _mapper.Map<User>(user);
        }

        /// <summary>
        /// Get the Chat that the Update was sent in.
        /// </summary>
        private Task<Chat> GetChat(long? chatId)
        {
            if (!chatId.HasValue)
                return Task.FromResult<Chat>(null);

            return _chatRepository.Get(chatId.Value);
        }

        /// <summary>
        /// Get the User that sent the Update.
        /// </summary>
        private Task<User> GetUser(long? userId)
        {
            if (!userId.HasValue)
                return Task.FromResult<User>(null);

            return _userRepository.Get(userId.Value);
        }

        /// <summary>
        /// Get the AuthorizationToken of the User that sent the Update.
        /// </summary>
        private Task<AuthorizationToken> GetAuthorizationToken(long? userId)
        {
            if (!userId.HasValue)
                return Task.FromResult<AuthorizationToken>(null);

            return _authorizationTokenRepository.Get(userId.Value);
        }

        /// <summary>
        /// Get the Playlist that was set for this Chat.
        /// </summary>
        private Task<Playlist> GetPlaylist(Chat chat)
        {
            if (chat == null || string.IsNullOrEmpty(chat?.PlaylistId))
                return Task.FromResult<Playlist>(null);

            return _playlistRepository.Get(chat.PlaylistId);
        }
    }
}
