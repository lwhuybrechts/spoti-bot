using Spoti_bot.Bot.Chats;
using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify;
using Spoti_bot.Spotify.Api;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Playlists;
using Spoti_bot.Spotify.Tracks.SyncTracks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Bot.HandleUpdate
{
    public class HandleCommandService : IHandleCommandService
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
            ISpotifyClientFactory spotifyClientFactory,
            ISpotifyClientService spotifyClientService)
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
        /// Check if the message contains a command.
        /// </summary>
        public bool IsAnyCommand(Message message)
        {
            return _commandsService.IsAnyCommand<Command>(message?.Text);
        }

        /// <summary>
        /// Check if a message contains a command and if so, handle it.
        /// </summary>
        /// <param name="message">The message to check for commands.</param>
        /// <param name="chat">The chat the message was posted in.</param>
        /// <returns>A BotResponseCode if a command was handled, or NoAction if no matching command was found.</returns>
        public async Task<BotResponseCode> TryHandleCommand(Message message, Chats.Chat chat)
        {
            var playlist = await GetPlaylist(chat);

            foreach (var command in Enum.GetValues(typeof(Command)).Cast<Command>())
                if (_commandsService.IsCommand(message.Text, command))
                {
                    var responseCode = await HandleCommand(command, message, playlist, chat);
                    if (responseCode != BotResponseCode.NoAction)
                        return responseCode;
                }

            return BotResponseCode.NoAction;
        }

        /// <summary>
        /// Handle a command and respond in the chat.
        /// </summary>
        private async Task<BotResponseCode> HandleCommand(Command command, Message message, Playlist playlist, Chats.Chat chat)
        {
            var text = await ValidateRequirements(command, message, playlist, chat);

            if (!string.IsNullOrEmpty(text))
            {
                await _sendMessageService.SendTextMessage(message.Chat.Id, text);
                return BotResponseCode.CommandRequirementNotFulfilled;
            }

            return command switch
            {
                Command.Test => await HandleTest(message),
                Command.Help => await HandleHelp(message, playlist),
                Command.Start => await HandleStart(message),
                Command.GetLoginLink => await HandleGetLoginLink(message, playlist, chat),
                Command.ResetPlaylistStorage => await HandleResetPlaylistStorage(message, chat),
                Command.SetPlaylist => await HandleSetPlaylist(message, chat),
                _ => throw new NotImplementedException($"Command {command} has no handle function defined."),
            };
        }

        /// <summary>
        /// Validates if all the commands requirements were met.
        /// </summary>
        /// <returns>An empty string if all requirements were met, or an error message.</returns>
        private async Task<string> ValidateRequirements(Command command, Message message, Playlist playlist, Chats.Chat chat)
        {
            // TODO: replace with fluent validation.
            if (command.RequiresChat() && chat == null)
                return $"Spoti-bot first needs to be added to this chat by sending the {Command.Start.ToDescriptionString()} command.";

            if (command.RequiresNoChat() && chat != null)
            {
                var admin = await _userService.Get(chat.AdminUserId);

                // A chat should always have an admin.
                if (admin == null)
                    throw new ChatAdminNullException(chat.Id, chat.AdminUserId);

                if (message.From.Id != admin.Id)
                    return $"Spoti-bot is already added to this chat, {admin.FirstName} is it's admin.";
                else
                    return $"Spoti-bot is already added to this chat, you are it's admin.";
            }

            if (command.RequiresChatAdmin())
            {
                if (chat == null)
                    return $"Spoti-bot first needs to be added to this chat by sending the {Command.Start.ToDescriptionString()} command.";

                var admin = await _userService.Get(chat.AdminUserId);

                // A chat should always have an admin.
                if (admin == null)
                    throw new ChatAdminNullException(chat.Id, chat.AdminUserId);

                if (message.From.Id != admin.Id)
                    return $"Only the chat admin ({admin.FirstName}) can use this command.";
            }

            if (command.RequiresPlaylist() && playlist == null)
                return $"Please set a playlist first, with command {Command.SetPlaylist.ToDescriptionString()}.";

            if (command.RequiresNoPlaylist() && playlist != null)
                return $"This chat already has a { _spotifyLinkHelper.GetMarkdownLinkToPlaylist(playlist.Id, "playlist")} set.";

            if (command.RequiresQuery() && !_commandsService.HasQuery(message.Text, command))
                return $"Please add a query after the {command.ToDescriptionString()} command.";

            return string.Empty;
        }

        /// <summary>
        /// The test command can be used to check if the bot is running. The bot responds with a silly response in the chat.
        /// </summary>
        private async Task<BotResponseCode> HandleTest(Message message)
        {
            var responses = new[]
            {
                $"Hi {Command.Test.ToDescriptionString()}, how's it going?",
                "Beep boop, I'm a bot.",
                "Spoti-bot is live and ready."
            };
            var randomIndex = new Random().Next(responses.Length);

            await _sendMessageService.SendTextMessage(message.Chat.Id, responses[randomIndex]);
            return BotResponseCode.TestCommandHandled;
        }

        /// <summary>
        /// The help command tells users what the bot can do and provides a link to the spotify playlist.
        /// </summary>
        private async Task<BotResponseCode> HandleHelp(Message message, Playlist playlist)
        {
            var text = "Welcome to Spoti-bot.\n\n" +
                $"Post links to Spotify tracks in this chat and they will be added to the playlist {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(playlist.Id, playlist.Name)}.";
            
            await _sendMessageService.SendTextMessage(message.Chat.Id, text, disableWebPagePreview: false);
            return BotResponseCode.HelpCommandHandled;
        }

        /// <summary>
        /// The start command will be triggered when the bot is first added to a chat.
        /// It sets the user that sent the Start command as the admin of spoti-bot for this chat.
        /// </summary>
        private async Task<BotResponseCode> HandleStart(Message message)
        {
            // Save the user in storage.
            var admin = await _userService.SaveUser(message.From);
            
            // Save the chat in storage.
            await _chatRepository.Upsert(new Chats.Chat
            {
                Id = message.Chat.Id,
                Title = message.Chat.Title,
                AdminUserId = admin.Id
            });

            var responseText = $"Spoti-bot is added to the chat, {admin.FirstName} is it's admin.";

            // Check if the user already is logged in to Spotify.
            if (_authorizationTokenRepository.Get(admin.Id) == null)
                responseText += $"\n\nPlease login to Spotify by sending the {Command.GetLoginLink.ToDescriptionString()} command.";
            else
                responseText += $"\n\nPlease set the spotify playlist for this chat by sending the {Command.SetPlaylist.ToDescriptionString()} command.";

            await _sendMessageService.SendTextMessage(message.Chat.Id, responseText);
            return BotResponseCode.StartCommandHandled;
        }

        /// <summary>
        /// The GetLoginLink command will let the admin login to spotify and save it's accesstoken.
        /// </summary>
        private async Task<BotResponseCode> HandleGetLoginLink(Message message, Playlist playlist, Chats.Chat chat)
        {
            // TODO: Only send the loginLink to private chats.
            var responseText = $"Please login to authorize Spoti-Bot.\n" +
            $"It needs access to the playlist {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(playlist.Id, playlist.Name)} and to your queue.";

            var loginRequestUri = await _spotifyAuthorizationService.CreateLoginRequest(chat.AdminUserId);
            var keyboard = _keyboardService.CreateButtonKeyboard("Login to Spotify", loginRequestUri.ToString());

            await _sendMessageService.SendTextMessage(message.Chat.Id, responseText, replyMarkup: keyboard);
            return BotResponseCode.GetLoginLinkCommandHandled;
        }

        /// <summary>
        /// The set playlist command can be used by the chat admin to set the playlist that tracks are added to in this chat.
        /// </summary>
        private async Task<BotResponseCode> HandleSetPlaylist(Message message, Chats.Chat chat)
        {
            // Get playlistId from command query.
            var query = _commandsService.GetQuery(message.Text, Command.SetPlaylist);
            var playlistId = await _spotifyLinkHelper.ParsePlaylistId(query);

            if (string.IsNullOrEmpty(playlistId))
            {
                var text = $"The playlist link could not be found in your query.";
                await _sendMessageService.SendTextMessage(message.Chat.Id, text);
                return BotResponseCode.SetPlaylistCommandHandled;
            }

            var spotifyClient = await _spotifyClientFactory.Create(chat.AdminUserId);

            if (spotifyClient == null)
            {
                var text = $"Spoti-bot is not authorized to set the Playlist for this chat. Please authorize Spoti-bot by sending the {Command.GetLoginLink.ToDescriptionString()} command.";
                await _sendMessageService.SendTextMessage(message.Chat.Id, text);
                return BotResponseCode.SetPlaylistCommandHandled;
            }
            
            // Get the playlist info from the spotify api.
            var playlist = await _spotifyClientService.GetPlaylist(spotifyClient, playlistId);
            if (playlist == null)
                throw new PlaylistNullException(playlistId);

            // Save the playlist to storage.
            playlist = await _playlistRepository.Upsert(playlist);

            // Save the playlist id with the current chat.
            chat.PlaylistId = playlist.Id;
            await _chatRepository.Upsert(chat);

            var responseText = $"Playlist {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(playlist.Id, playlist.Name)} successfully set for this chat. You can now post Spotify track urls in this chat.\n\nEnjoy Spoti-bot!";
            await _sendMessageService.SendTextMessage(message.Chat.Id, responseText);
            return BotResponseCode.SetPlaylistCommandHandled;
        }

        /// <summary>
        /// The ResetPlaylistStorage command updates track information in storage with data from the spotify api.
        /// </summary>
        private async Task<BotResponseCode> HandleResetPlaylistStorage(Message message, Chats.Chat chat)
        {
            await _syncTracksService.SyncTracks(chat);
            var responseText = "Spoti-bot playlist storage has been synced.";

            await _sendMessageService.SendTextMessage(message.Chat.Id, responseText);
            return BotResponseCode.ResetCommandHandled;
        }

        /// <summary>
        /// Get the playlist that was set for this chat.
        /// </summary>
        private Task<Playlist> GetPlaylist(Chats.Chat chat)
        {
            if (string.IsNullOrEmpty(chat?.PlaylistId))
                return Task.FromResult<Playlist>(null);

            return _playlistRepository.Get(chat.PlaylistId);
        }
    }
}
