using Spoti_bot.Bot.Chats;
using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify;
using Spoti_bot.Spotify.Api;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Playlists;
using Spoti_bot.Spotify.Tracks.SyncTracks;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    public class HandleCommandService : BaseCommandsService<Command>, IHandleCommandService
    {
        private readonly ICommandsService _commandsService;
        private readonly IAuthorizationService _spotifyAuthorizationService;
        private readonly IAuthorizationTokenRepository _authorizationTokenRepository;
        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly ISyncTracksService _syncTracksService;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IKeyboardService _keyboardService;
        private readonly IChatRepository _chatRepository;
        private readonly IUserService _userService;
        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly ISpotifyClientService _spotifyClientService;

        public HandleCommandService(
            ICommandsService commandsService,
            IAuthorizationService spotifyAuthorizationService,
            IAuthorizationTokenRepository authorizationTokenRepository,
            ISendMessageService sendMessageService,
            ISpotifyLinkHelper spotifyLinkHelper,
            ISyncTracksService trackService,
            IPlaylistRepository playlistRepository,
            IKeyboardService keyboardService,
            IChatRepository chatRepository,
            IUserService userService,
            IUserRepository userRepository,
            ISpotifyClientFactory spotifyClientFactory,
            ISpotifyClientService spotifyClientService)
            : base(commandsService, userRepository, sendMessageService, spotifyLinkHelper)
        {
            _commandsService = commandsService;
            _spotifyAuthorizationService = spotifyAuthorizationService;
            _authorizationTokenRepository = authorizationTokenRepository;
            _sendMessageService = sendMessageService;
            _spotifyLinkHelper = spotifyLinkHelper;
            _syncTracksService = trackService;
            _playlistRepository = playlistRepository;
            _keyboardService = keyboardService;
            _chatRepository = chatRepository;
            _userService = userService;
            _spotifyClientFactory = spotifyClientFactory;
            _spotifyClientService = spotifyClientService;
        }

        /// <summary>
        /// Handle a command and respond in the chat.
        /// </summary>
        protected override Task<BotResponseCode> HandleCommand(Command command, UpdateDto updateDto)
        {
            return command switch
            {
                Command.Test => HandleTest(updateDto),
                Command.Help => HandleHelp(updateDto),
                Command.Start => updateDto.ParsedChat?.Type == ChatType.Private
                    ? HandleStartInPrivateChat(updateDto)
                    : HandleStartInGroupChat(updateDto),
                Command.GetLoginLink => HandleGetLoginLink(updateDto),
                Command.ResetPlaylistStorage => HandleResetPlaylistStorage(updateDto),
                Command.SetPlaylist => HandleSetPlaylist(updateDto),
                _ => throw new NotImplementedException($"Command {command} has no handle function defined.")
            };
        }

        /// <summary>
        /// The test command can be used to check if the bot is running. The bot responds with a silly response in the chat.
        /// </summary>
        private async Task<BotResponseCode> HandleTest(UpdateDto updateDto)
        {
            var responses = new[]
            {
                $"Hi {Command.Test.ToDescriptionString()}, how's it going?",
                "Beep boop, I'm a bot.",
                "Spoti-bot is live and ready."
            };
            var randomIndex = new Random().Next(responses.Length);

            await _sendMessageService.SendTextMessage(updateDto.ParsedChat.Id, responses[randomIndex]);
            return BotResponseCode.TestCommandHandled;
        }

        /// <summary>
        /// The help command tells users what the bot can do and provides a link to the spotify playlist.
        /// </summary>
        private async Task<BotResponseCode> HandleHelp(UpdateDto updateDto)
        {
            var text = "Welcome to Spoti-bot.\n\n" +
                $"Post links to Spotify tracks in this chat and they will be added to the playlist {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(updateDto.Playlist.Id, updateDto.Playlist.Name)}.";

            await _sendMessageService.SendTextMessage(updateDto.ParsedChat.Id, text, disableWebPagePreview: false);
            return BotResponseCode.HelpCommandHandled;
        }

        /// <summary>
        /// The start command in a private chat is triggered after the start command in a group chat.
        /// The chatId should be added as a query.
        /// </summary>
        private async Task<BotResponseCode> HandleStartInPrivateChat(UpdateDto updateDto)
        {
            // The Start command in private chats should have a chatId as a query.
            if (!_commandsService.HasQuery(updateDto.ParsedTextMessage, Command.Start))
                return BotResponseCode.CommandRequirementNotFulfilled;

            if (!int.TryParse(_commandsService.GetQuery(updateDto.ParsedTextMessage, Command.Start), out var chatId))
                return BotResponseCode.CommandRequirementNotFulfilled;
            
            return await HandleGetLoginLink(updateDto, chatId);
        }

        /// <summary>
        /// The start command will be triggered when the bot is first added to a chat.
        /// It sets the user that sent the Start command as the admin of Spoti-Bot for this chat.
        /// </summary>
        private async Task<BotResponseCode> HandleStartInGroupChat(UpdateDto updateDto)
        {
            // Save the user in storage.
            var admin = await _userService.SaveUser(updateDto.ParsedUser, updateDto.ParsedChat.Id);

            // Save the chat in storage.
            var chat = await _chatRepository.Upsert(new Chat
            {
                Id = updateDto.ParsedChat.Id,
                Title = updateDto.ParsedChat.Title,
                AdminUserId = admin.Id,
                Type = updateDto.ParsedChat.Type
            });

            var responseText = $"Spoti-bot is added to the chat, {admin.FirstName} is it's admin.";

            IReplyMarkup keyboard = null;
            // Check if the user has already logged in to Spotify.
            if (_authorizationTokenRepository.Get(admin.Id) != null)
                responseText += $"\n\nPlease set the spotify playlist for this chat by sending the {Command.SetPlaylist.ToDescriptionString()} command.";
            else
            {
                keyboard = _keyboardService.CreateSwitchToPmKeyboard(chat);
                responseText += $"\n\nThe next step is to connect your Spotify account. Click the button below and then click _Connect in private chat_.\n\nThis switches you to a private chat where you can login.";
            }

            await _sendMessageService.SendTextMessage(chat.Id, responseText, replyMarkup: keyboard);
            return BotResponseCode.StartCommandHandled;
        }

        /// <summary>
        /// The GetLoginLink command will let the admin login to spotify and save it's accesstoken.
        /// </summary>
        private async Task<BotResponseCode> HandleGetLoginLink(UpdateDto updateDto, long? groupChatId = null)
        {
            if (groupChatId.HasValue)
            {
                // Only the admin can request a login link for a group.
                var groupChat = await _chatRepository.Get(groupChatId.Value);
                if (groupChat == null || groupChat.AdminUserId != updateDto.ParsedUser.Id)
                    return BotResponseCode.CommandRequirementNotFulfilled;
            }

            var responseText = "Please login to authorize Spoti-Bot.\nIt needs access to your playlists and to your queue.";

            var loginRequestUri = await _spotifyAuthorizationService.CreateLoginRequest(updateDto.ParsedUser.Id, groupChatId, updateDto.ParsedChat.Id);
            
            var keyboard = _keyboardService.CreateButtonKeyboard("Login to Spotify", loginRequestUri.ToString());

            await _sendMessageService.SendTextMessage(updateDto.ParsedChat.Id, responseText, replyMarkup: keyboard);

            return BotResponseCode.GetLoginLinkCommandHandled;
        }

        /// <summary>
        /// The set playlist command can be used by the chat admin to set the playlist that tracks are added to in this chat.
        /// </summary>
        private async Task<BotResponseCode> HandleSetPlaylist(UpdateDto updateDto)
        {
            // Get playlistId from command query.
            var query = _commandsService.GetQuery(updateDto.ParsedTextMessage, Command.SetPlaylist);
            var playlistId = await _spotifyLinkHelper.ParsePlaylistId(query);

            if (string.IsNullOrEmpty(playlistId))
            {
                var text = $"The playlist link could not be found in your query.";
                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, text);
                return BotResponseCode.CommandRequirementNotFulfilled;
            }

            var spotifyClient = await _spotifyClientFactory.Create(updateDto.Chat.AdminUserId);

            if (spotifyClient == null)
            {
                var text = $"Spoti-bot is not authorized to set the Playlist for this chat. Please authorize Spoti-bot first, by sending the {Command.Start.ToDescriptionString()} command.";
                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, text);
                return BotResponseCode.CommandRequirementNotFulfilled;
            }

            // Get the playlist info from the spotify api.
            var playlist = await _spotifyClientService.GetPlaylist(spotifyClient, playlistId);
            if (playlist == null)
                throw new PlaylistNullException(playlistId);

            // Save the playlist to storage.
            playlist = await _playlistRepository.Upsert(playlist);

            // Save the playlist id with the current chat.
            updateDto.Chat.PlaylistId = playlist.Id;
            await _chatRepository.Upsert(updateDto.Chat);

            var responseText = $"Playlist {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(playlist.Id, playlist.Name)} successfully set for this chat. You can now post Spotify track urls in this chat.\n\nEnjoy Spoti-bot!";
            await _sendMessageService.SendTextMessage(updateDto.Chat.Id, responseText);
            return BotResponseCode.SetPlaylistCommandHandled;
        }

        /// <summary>
        /// The ResetPlaylistStorage command updates track information in storage with data from the spotify api.
        /// </summary>
        private async Task<BotResponseCode> HandleResetPlaylistStorage(UpdateDto updateDto)
        {
            // TODO: also sync playlist name.
            await _syncTracksService.SyncTracks(updateDto.Chat);
            var responseText = "Spoti-bot playlist storage has been synced.";

            await _sendMessageService.SendTextMessage(updateDto.Chat.Id, responseText);
            return BotResponseCode.ResetCommandHandled;
        }
    }
}
