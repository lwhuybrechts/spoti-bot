using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Spoti_bot.Bot;
using Spoti_bot.Bot.Chats;
using Spoti_bot.Bot.HandleUpdate;
using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Bot.Votes;
using Spoti_bot.IntegrationTests.Library;
using Spoti_bot.Library;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify;
using Spoti_bot.Spotify.Api;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Playlists;
using Spoti_bot.Spotify.Tracks;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Spoti_bot.IntegrationTests
{
    public class UpdateTests : IClassFixture<TestHost>
    {
        private readonly Update _sut;

        private readonly GenerateUpdateStreamService _generateUpdateStreamService;
        private readonly TestOptions _testOptions;

        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ITrackRepository _trackRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IChatMemberRepository _chatMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IVoteTextHelper _voteTextHelper;
        private readonly ILoginRequestRepository _loginRequestRepository;
        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;

        public UpdateTests(TestHost testHost)
        {
            _generateUpdateStreamService = testHost.GetService<GenerateUpdateStreamService>();
            _testOptions = testHost.GetService<IOptions<TestOptions>>().Value;

            _spotifyClientFactory = testHost.GetService<ISpotifyClientFactory>();
            _spotifyClientService = testHost.GetService<ISpotifyClientService>();
            _trackRepository = testHost.GetService<ITrackRepository>();
            _playlistRepository = testHost.GetService<IPlaylistRepository>();
            _chatRepository = testHost.GetService<IChatRepository>();
            _chatMemberRepository= testHost.GetService<IChatMemberRepository>();
            _userRepository = testHost.GetService<IUserRepository>();
            _voteRepository = testHost.GetService<IVoteRepository>();
            _voteTextHelper = testHost.GetService<IVoteTextHelper>();
            _loginRequestRepository = testHost.GetService<ILoginRequestRepository>();
            _sendMessageService = testHost.GetService<ISendMessageService>();
            _spotifyLinkHelper = testHost.GetService<ISpotifyLinkHelper>();

            var handleMessageService = testHost.GetService<IHandleMessageService>();
            var handleCallbackQueryService = testHost.GetService<IHandleCallbackQueryService>();
            var handleInlineQueryService = testHost.GetService<IHandleInlineQueryService>();
            var updateDtoService = testHost.GetService<IUpdateDtoService>();
            var sentryOptions = testHost.GetService<IOptions<SentryOptions>>();

            _sut = new Update(handleMessageService, handleCallbackQueryService, handleInlineQueryService, updateDtoService, sentryOptions);
        }

        private async Task TruncateTables()
        {
            // TODO: use dev tables and do actual truncates.
            // Remove specific deletes in these tests since the tables are truncated.
            await DeleteTrack();
            await DeletePlaylist();
            await DeleteChat();
            await DeleteUser();
            await DeleteVotes();
            await DeleteLoginRequests();
            await DeleteChatMembers();
        }

        [Fact]
        public async Task Run_NoRequestBody_NoActionReturned()
        {
            // Arrange.
            var httpContext = new DefaultHttpContext();

            // Act.
            var result = await _sut.Run(httpContext.Request);

            // Assert.
            AssertHelper.Equal(BotResponseCode.NoAction, result);
        }

        [Fact]
        public async Task Run_NoTextString_NoActionReturned()
        {
            // Arrange.
            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, "");

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.NoAction, result);
        }

        [Fact]
        public async Task Run_TestCommand_TestCommandHandledReturned()
        {
            // Arrange.
            await TruncateTables();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.Test.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.TestCommandHandled, result);
        }

        [Fact]
        public async Task Run_HelpCommand_NoChat_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertPlaylist();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.Help.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_HelpCommand_NoPlaylist_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.Help.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_HelpCommand_HelpCommandHandledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertPlaylist();
            await InsertChat();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.Help.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.HelpCommandHandled, result);
        }

        [Fact]
        public async Task Run_StartCommand_HasChat_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.Start.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_StartCommand_StartCommandHandledReturned()
        {
            // Arrange.
            await TruncateTables();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.Start.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.StartCommandHandled, result);
        }

        [Fact]
        public async Task Run_StartCommand_UserAndChatInserted()
        {
            // Arrange.
            await TruncateTables();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.Start.ToDescriptionString());

            // Act.
            await _sut.Run(httpRequest);

            // Assert.
            var user = await _userRepository.Get(_testOptions.TestUserId);
            Assert.NotNull(user);

            var chat = await _chatRepository.Get(_testOptions.TestChatId);
            Assert.NotNull(chat);

            var chatMember = await _chatMemberRepository.Get(_testOptions.TestUserId, _testOptions.TestChatId.ToString());
            Assert.NotNull(chatMember);
        }

        [Fact]
        public async Task Run_GetLoginLinkCommand_NoChat_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertPlaylist();
            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.GetLoginLink.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_GetLoginLinkCommand_NoPlaylist_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.GetLoginLink.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_GetLoginLinkCommand_NoUser_ExceptionHandledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertPlaylist();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.GetLoginLink.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.ExceptionHandled, result);
        }

        [Fact]
        public async Task Run_GetLoginLinkCommand_TestUserNotAdmin_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            var user = await InsertUser();
            
            var otherUserId = user.Id - 1;
            var otherUser = await InsertUser(otherUserId);
            
            await InsertChat(otherUser.Id) ;
            await InsertPlaylist();
            
            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.GetLoginLink.ToDescriptionString());

            try
            {
                // Act.
                var result = await _sut.Run(httpRequest);

                // Assert.
                AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
            }
            finally
            {
                await DeleteUser(otherUser);
            }
        }

        [Fact]
        public async Task Run_GetLoginLinkCommand_LoginLinkCommandHandledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertPlaylist();
            await InsertChat();
            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.GetLoginLink.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.GetLoginLinkCommandHandled, result);
        }

        [Fact]
        public async Task Run_GetLoginLinkCommand_LoginRequestInserted()
        {
            // Arrange.
            await TruncateTables();

            await InsertPlaylist();
            await InsertChat();
            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.GetLoginLink.ToDescriptionString());

            // Act.
            await _sut.Run(httpRequest);

            // Assert.
            var allLoginRequests = await _loginRequestRepository.GetAll();
            var chatLoginRequest = allLoginRequests.Where(x => x.ChatId == _testOptions.TestChatId).Single();

            Assert.Equal(chatLoginRequest.UserId, _testOptions.TestUserId);
        }

        [Fact]
        public async Task Run_ResetPlaylistStorage_NoChat_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();
            
            await InsertPlaylist();
            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.ResetPlaylistStorage.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_ResetPlaylistStorage_NoPlaylist_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.ResetPlaylistStorage.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_ResetPlaylistStorage_NoUser_ExceptionHandledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertPlaylist();
            await InsertChat();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.ResetPlaylistStorage.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.ExceptionHandled, result);
        }

        [Fact]
        public async Task Run_ResetPlaylistStorage_TestUserNotAdmin_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            var user = await InsertUser();

            var otherUserId = user.Id - 1;
            var otherUser = await InsertUser(otherUserId);

            await InsertChat(otherUser.Id);
            await InsertPlaylist();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.ResetPlaylistStorage.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_ResetPlaylistStorage_ResetCommandHandledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertPlaylist();
            await InsertChat();
            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.ResetPlaylistStorage.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.ResetCommandHandled, result);
        }

        [Fact]
        public async Task Run_SetPlaylist_NoChat_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, $"{Command.SetPlaylist.ToDescriptionString()} {_testOptions.TestPlaylistId}");

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_SetPlaylist_NoUser_ExceptoinHandledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, $"{Command.SetPlaylist.ToDescriptionString()} {_testOptions.TestPlaylistId}");

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.ExceptionHandled, result);
        }

        [Fact]
        public async Task Run_SetPlaylist_TestUserNotAdmin_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            var user = await InsertUser();

            var otherUserId = user.Id - 1;
            var otherUser = await InsertUser(otherUserId);

            await InsertChat(otherUser.Id);

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, $"{Command.SetPlaylist.ToDescriptionString()} {_testOptions.TestPlaylistId}");

            try
            {
                // Act.
                var result = await _sut.Run(httpRequest);

                // Assert.
                AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
            }
            finally
            {
                await DeleteUser(otherUser);
            }
        }

        [Fact]
        public async Task Run_SetPlaylist_HasPlaylist_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertUser();
            await InsertPlaylist();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, $"{Command.SetPlaylist.ToDescriptionString()} {_testOptions.TestPlaylistId}");

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_SetPlaylist_NoQuery_CommandRequirementNotFulfilledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertUser();

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.SetPlaylist.ToDescriptionString());
            
            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_SetPlaylist_SetPlaylistCommandHandledReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertUser();
            
            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, $"{Command.SetPlaylist.ToDescriptionString()} {_testOptions.TestPlaylistId}");

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.SetPlaylistCommandHandled, result);
        }

        [Fact]
        public async Task Run_SetPlaylist_PlaylistInserted()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertUser();

            var playlistUrl = _spotifyLinkHelper.GetLinkToPlaylist(_testOptions.TestPlaylistId);

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, $"{Command.SetPlaylist.ToDescriptionString()} {playlistUrl}");

            // Act.
            await _sut.Run(httpRequest);

            // Assert.
            var playlist = await _playlistRepository.Get(_testOptions.TestPlaylistId);
            Assert.NotNull(playlist);

            var chat = await _chatRepository.Get(_testOptions.TestChatId);
            Assert.Equal(_testOptions.TestPlaylistId, chat.PlaylistId);
        }

        [Fact]
        public async Task Run_AddTrack_NoChat_NoActionReturned()
        {
            // Arrange.
            await TruncateTables();

            var trackUrl = _spotifyLinkHelper.GetLinkToTrack(_testOptions.TestTrackId);

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, trackUrl);

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.NoAction, result);
        }

        [Fact]
        public async Task Run_AddTrack_NoPlaylistInChat_NoActionReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat(addPlaylistId: false);

            var trackUrl = _spotifyLinkHelper.GetLinkToTrack(_testOptions.TestTrackId);

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, trackUrl);

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.NoAction, result);
        }

        [Fact]
        public async Task Run_AddTrack_NoPlaylist_NoActionReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();

            var trackUrl = _spotifyLinkHelper.GetLinkToTrack(_testOptions.TestTrackId);

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, trackUrl);

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.NoAction, result);
        }

        [Fact]
        public async Task Run_AddTrackThatAlreadyExists_TrackAlreadyExistsReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertPlaylist();

            var track = await InsertTrack();
            var trackUrlThatAlreadyExists = _spotifyLinkHelper.GetLinkToTrack(track.Id);

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, trackUrlThatAlreadyExists);

            try
            {
                // Act.
                var result = await _sut.Run(httpRequest);

                // Assert.
                AssertHelper.Equal(BotResponseCode.TrackAlreadyExists, result);
            }
            finally
            {
                // Remove the track from storage.
                await _trackRepository.Delete(track);
            }
        }

        [Fact]
        public async Task Run_AddTrack_TrackAddedToPlaylistReturned()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertPlaylist();

            var trackUrl = _spotifyLinkHelper.GetLinkToTrack(_testOptions.TestTrackId);

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, trackUrl);

            try
            {
                // Act.
                var result = await _sut.Run(httpRequest);

                // Assert.
                AssertHelper.Equal(BotResponseCode.TrackAddedToPlaylist, result);
            }
            finally
            {
                // TODO: make sure the testuser has an access token.
                var spotifyClient = await _spotifyClientFactory.Create(_testOptions.TestUserId);

                // Make sure the track is deleted after the test is done.
                await _spotifyClientService.RemoveTrackFromPlaylist(spotifyClient, _testOptions.TestTrackId, _testOptions.TestPlaylistId);
                
                // Remove the track from storage.
                var track = await _trackRepository.Get(_testOptions.TestTrackId, _testOptions.TestPlaylistId);
                await _trackRepository.Delete(track);
            }
        }

        [Fact]
        public async Task Run_AddTrack_UserInserted()
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertPlaylist();

            var trackUrl = _spotifyLinkHelper.GetLinkToTrack(_testOptions.TestTrackId);

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, trackUrl);

            try
            {
                // Act.
                var result = await _sut.Run(httpRequest);

                // Assert.
                AssertHelper.Equal(BotResponseCode.TrackAddedToPlaylist, result);

                var track = await _trackRepository.Get(_testOptions.TestTrackId, _testOptions.TestPlaylistId.ToString());
                Assert.NotNull(track);

                var user = await _userRepository.Get(_testOptions.TestUserId);
                Assert.NotNull(user);

                var chatMember = await _chatMemberRepository.Get(_testOptions.TestUserId, _testOptions.TestChatId.ToString());
                Assert.NotNull(chatMember);
            }
            finally
            {
                // TODO: make sure the testuser has an access token.
                var spotifyClient = await _spotifyClientFactory.Create(_testOptions.TestUserId);

                // Make sure the track is deleted after the test is done.
                await _spotifyClientService.RemoveTrackFromPlaylist(spotifyClient, _testOptions.TestTrackId, _testOptions.TestPlaylistId);

                // Remove the track from storage.
                var track = await _trackRepository.Get(_testOptions.TestTrackId, _testOptions.TestPlaylistId);
                await _trackRepository.Delete(track);
            }
        }

        [Theory]
        [InlineData(VoteType.Upvote)]
        [InlineData(VoteType.Downvote)]
        public async Task Run_VoteCallback_NoChat_NoActionReturned(VoteType voteType)
        {
            // Arrange.
            await TruncateTables();

            await InsertPlaylist();

            var trackUrl = _spotifyLinkHelper.GetLinkToTrack(_testOptions.TestTrackId);

            // Send the two messages that go before a vote callback: one with a trackUrl and a reply from the bot with a vote button.
            var trackMessageId = await _sendMessageService.SendTextMessage(_testOptions.TestChatId, trackUrl);
            const string botReplyMessageText = "Track added to playlist!";
            var botReplyMessageId = await _sendMessageService.SendTextMessage(_testOptions.TestChatId, botReplyMessageText, replyToMessageId: trackMessageId);

            using var stream = new MemoryStream();
            var httpRequest = await CreateVoteCallbackQueryRequest(stream, voteType, botReplyMessageId, botReplyMessageText, trackUrl);

            // Act.
            var voteResult = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.NoAction, voteResult);
        }

        [Theory]
        [InlineData(VoteType.Upvote)]
        [InlineData(VoteType.Downvote)]
        public async Task Run_VoteCallback_NoPlaylist_NoActionReturned(VoteType voteType)
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();

            var trackUrl = _spotifyLinkHelper.GetLinkToTrack(_testOptions.TestTrackId);

            // Send the two messages that go before a vote callback: one with a trackUrl and a reply from the bot with a vote button.
            var trackMessageId = await _sendMessageService.SendTextMessage(_testOptions.TestChatId, trackUrl);
            const string botReplyMessageText = "Track added to playlist!";
            var botReplyMessageId = await _sendMessageService.SendTextMessage(_testOptions.TestChatId, botReplyMessageText, replyToMessageId: trackMessageId);

            using var stream = new MemoryStream();
            var httpRequest = await CreateVoteCallbackQueryRequest(stream, voteType, botReplyMessageId, botReplyMessageText, trackUrl);

            // Act.
            var voteResult = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.NoAction, voteResult);
        }

        [Theory]
        [InlineData(VoteType.Upvote)]
        [InlineData(VoteType.Downvote)]
        public async Task Run_VoteCallback_NoTrack_NoActionReturned(VoteType voteType)
        {
            // Arrange.
            await TruncateTables();

            await InsertChat();
            await InsertPlaylist();

            var trackUrl = _spotifyLinkHelper.GetLinkToTrack(_testOptions.TestTrackId);

            // Send the two messages that go before a vote callback: one with a trackUrl and a reply from the bot with a vote button.
            var trackMessageId = await _sendMessageService.SendTextMessage(_testOptions.TestChatId, trackUrl);
            const string botReplyMessageText = "Track added to playlist!";
            var botReplyMessageId = await _sendMessageService.SendTextMessage(_testOptions.TestChatId, botReplyMessageText, replyToMessageId: trackMessageId);

            using var stream = new MemoryStream();
            var httpRequest = await CreateVoteCallbackQueryRequest(stream, voteType, botReplyMessageId, botReplyMessageText, trackUrl);

            // Act.
            var voteResult = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.NoAction, voteResult);
        }

        [Theory]
        [InlineData(VoteType.Upvote)]
        [InlineData(VoteType.Downvote)]
        public async Task Run_VoteCallbackTwice_AddVoteHandledReturned_RemoveVoteHandledReturned(VoteType voteType)
        {
            // Arrange.
            await TruncateTables();

            await InsertTrack();
            await InsertPlaylist();
            await InsertChat();

            var trackUrl = _spotifyLinkHelper.GetLinkToTrack(_testOptions.TestTrackId);

            // Send the two messages that go before an vote callback: one with a trackUrl and a reply from the bot with a vote button.
            var trackMessageId = await _sendMessageService.SendTextMessage(_testOptions.TestChatId, trackUrl);
            var botReplyMessageText = "Track added to the playlist!";
            var botReplyMessageId = await _sendMessageService.SendTextMessage(_testOptions.TestChatId, botReplyMessageText, replyToMessageId: trackMessageId);

            // Send two callback updates, first to test to add a vote...
            using (var stream = new MemoryStream())
            {
                var httpRequest = await CreateVoteCallbackQueryRequest(stream, voteType, botReplyMessageId, botReplyMessageText, trackUrl);

                // Act.
                var voteResult = await _sut.Run(httpRequest);

                // Assert.
                AssertHelper.Equal(BotResponseCode.AddVoteHandled, voteResult);

                var expectedVote = new Vote
                {
                    PlaylistId = _testOptions.TestPlaylistId,
                    UserId = _testOptions.TestUserId,
                    TrackId = _testOptions.TestTrackId,
                    Type = voteType
                };
                
                var vote = await _voteRepository.Get(expectedVote);
                Assert.NotNull(vote);

                var user = await _userRepository.Get(_testOptions.TestUserId);
                Assert.NotNull(user);

                var chatMember = await _chatMemberRepository.Get(_testOptions.TestUserId, _testOptions.TestChatId.ToString());
                Assert.NotNull(chatMember);
            }

            // ...and then test to remove a vote.
            using (var stream = new MemoryStream())
            {
                // Arrange.
                botReplyMessageText = voteType.HasAttribute<VoteType, VoteAttributes.UseNegativeOperatorAttribute>()
                    ? _voteTextHelper.DecrementVote(botReplyMessageText, voteType)
                    : _voteTextHelper.IncrementVote(botReplyMessageText, voteType);
                var httpRequest = await CreateVoteCallbackQueryRequest(stream, voteType, botReplyMessageId, botReplyMessageText, trackUrl);

                // Act.
                var voteResult = await _sut.Run(httpRequest);

                // Assert.
                AssertHelper.Equal(BotResponseCode.RemoveVoteHandled, voteResult);

                var expectedVote = new Vote
                {
                    PlaylistId = _testOptions.TestPlaylistId,
                    UserId = _testOptions.TestUserId,
                    TrackId = _testOptions.TestTrackId,
                    Type = voteType
                };

                var vote = await _voteRepository.Get(expectedVote);
                Assert.Null(vote);
            }
        }

        [Fact]
        public async Task Run_UpvoteInlineQuery_NoQuery_CommandRequirementNotFulfilledReturned()
        {
            // Arramge.
            await TruncateTables();

            using var stream = new MemoryStream();
            var httpRequest = await CreateInlineQueryRequest(stream, InlineQueryCommand.GetUpvoteUsers.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.CommandRequirementNotFulfilled, result);
        }

        [Fact]
        public async Task Run_UpvoteInlineQuery_OneQuery_InlineQueryHandledReturned()
        {
            // Arramge.
            await TruncateTables();

            var query = $"{InlineQueryCommand.GetUpvoteUsers.ToDescriptionString()} {_testOptions.TestTrackId}";

            using var stream = new MemoryStream();
            var httpRequest = await CreateInlineQueryRequest(stream, query);
            
            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.InlineQueryHandled, result);
        }

        [Fact]
        public async Task Run_UpvoteInlineQuery_TwoQueries_InlineQueryHandledReturned()
        {
            // Arramge.
            await TruncateTables();

            var query = $"{InlineQueryCommand.GetUpvoteUsers.ToDescriptionString()} {_testOptions.TestPlaylistId} {_testOptions.TestTrackId}";

            using var stream = new MemoryStream();
            var httpRequest = await CreateInlineQueryRequest(stream, query);

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.InlineQueryHandled, result);
        }

        private async Task<HttpRequest> CreateRequest(Stream stream, string textMessage)
        {
            await _generateUpdateStreamService.WriteTextMessageToStream(stream, textMessage);

            return CreateRequestWithBody(stream);
        }

        private async Task<HttpRequest> CreateVoteCallbackQueryRequest(Stream stream, VoteType voteType, int messageId, string messageText, string trackUrl)
        {
            await _generateUpdateStreamService.WriteVoteCallbackQueryToStream(stream, voteType, messageId, messageText, trackUrl);

            return CreateRequestWithBody(stream);
        }

        private async Task<HttpRequest> CreateInlineQueryRequest(Stream stream, string query)
        {
            await _generateUpdateStreamService.WriteInlineQueryToStream(stream, query);

            return CreateRequestWithBody(stream);
        }

        private HttpRequest CreateRequestWithBody(Stream bodyStream)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = bodyStream;

            return httpContext.Request;
        }

        private Task<Track> InsertTrack()
        {
            return _trackRepository.Upsert(CreateTrack());
        }

        private Task<Playlist> InsertPlaylist()
        {
            return _playlistRepository.Upsert(CreatePlaylist());
        }

        private Task<Chat> InsertChat(long? adminUserId = null, bool addPlaylistId = true)
        {
            return _chatRepository.Upsert(CreateChat(adminUserId, addPlaylistId));
        }

        private Task<User> InsertUser(long? id = null)
        {
            return _userRepository.Upsert(CreateUser(id));
        }

        private async Task DeleteTrack()
        {
            var track = await _trackRepository.Get(CreateTrack());

            if (track != null)
                await _trackRepository.Delete(track);
        }

        private async Task DeletePlaylist()
        {
            var playlist = await _playlistRepository.Get(CreatePlaylist());
            
            if (playlist != null)
                await _playlistRepository.Delete(playlist);
        }

        private async Task DeleteChat()
        {
            var chat = await _chatRepository.Get(CreateChat());
            
            if (chat != null)
                await _chatRepository.Delete(chat);
        }

        private async Task DeleteUser(User user = null)
        {
            if (user == null)
                user = await _userRepository.Get(CreateUser());

            if (user != null)
                await _userRepository.Delete(user);
        }

        private async Task DeleteVotes()
        {
            var votes = await _voteRepository.GetVotes(_testOptions.TestPlaylistId, _testOptions.TestTrackId);

            if (votes.Any())
                await _voteRepository.Delete(votes);
        }

        private async Task DeleteLoginRequests()
        {
            var loginRequests = await _loginRequestRepository.GetAll();

            var testLoginRequests = loginRequests.Where(x => x.ChatId == _testOptions.TestChatId).ToList();

            if (testLoginRequests.Any())
                await _loginRequestRepository.Delete(testLoginRequests);
        }

        private async Task DeleteChatMembers()
        {
            var chatMembers = await _chatMemberRepository.GetAllByPartitionKey(_testOptions.TestChatId.ToString());

            if (chatMembers.Any())
                await _chatMemberRepository.Delete(chatMembers);
        }

        private Track CreateTrack()
        {
            return new Track()
            {
                Id = _testOptions.TestTrackId,
                AddedByTelegramUserId = _testOptions.TestUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                PlaylistId = _testOptions.TestPlaylistId,
            };
        }

        private Playlist CreatePlaylist()
        {
            return new Playlist
            {
                Id = _testOptions.TestPlaylistId,
                Name = "Test Playlist"
            };
        }

        private Chat CreateChat(long? adminUserId = null, bool addPlaylistId = true)
        {
            var chat = new Chat
            {
                Id = _testOptions.TestChatId,
                AdminUserId = adminUserId ?? _testOptions.TestUserId
            };

            if (addPlaylistId)
                chat.PlaylistId = _testOptions.TestPlaylistId;

            return chat;
        }

        private User CreateUser(long? id = null)
        {
            return new User
            {
                Id = id ?? _testOptions.TestUserId,
                FirstName = "Spoti-bot test user"
            };
        }
    }
}
