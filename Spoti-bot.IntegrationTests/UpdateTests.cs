using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Spoti_bot.Bot;
using Spoti_bot.Bot.Commands;
using Spoti_bot.Bot.Interfaces;
using Spoti_bot.IntegrationTests.Library;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify.Data.Tracks;
using Spoti_bot.Spotify.Interfaces;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Spoti_bot.IntegrationTests
{
    public class UpdateTests : IClassFixture<TestHost>
    {
        private readonly Update _sut;

        private readonly GenerateUpdateStreamService _generateUpdateStreamService;
        private readonly TestOptions _testOptions;

        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ITrackRepository _trackRepository;
        private readonly ISendMessageService _sendMessageService;

        public UpdateTests(TestHost testHost)
        {
            _generateUpdateStreamService = testHost.GetService<GenerateUpdateStreamService>();
            _testOptions = testHost.GetService<TestOptions>();

            _spotifyClientService = testHost.GetService<ISpotifyClientService>();
            _trackRepository = testHost.GetService<ITrackRepository>();
            _sendMessageService = testHost.GetService<ISendMessageService>();

            var handleMessageService = testHost.GetService<IHandleMessageService>();
            var handleCallbackQueryService = testHost.GetService<IHandleCallbackQueryService>();
            var sentryOptions = testHost.GetService<IOptions<SentryOptions>>();

            _sut = new Update(handleMessageService, handleCallbackQueryService, sentryOptions);
        }

        [Fact]
        public async Task Update_NoRequestBody_NoActionReturned()
        {
            // Arrange.
            var httpContext = new DefaultHttpContext();

            // Act.
            var result = await _sut.Run(httpContext.Request);

            // Assert.
            AssertHelper.Equal(BotResponseCode.NoAction, result);
        }

        [Fact]
        public async Task Update_TestCommand_TestCommandHandledReturned()
        {
            // Arrange.
            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.Test.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.TestCommandHandled, result);
        }

        [Fact]
        public async Task Update_HelpCommand_HelpCommandHandledReturned()
        {
            // Arrange.
            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.Help.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.HelpCommandHandled, result);
        }

        [Fact]
        public async Task Update_GetLoginLinkCommand_LoginLinkCOmmandHandledReturned()
        {
            // Arrange.
            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, Command.GetLoginLink.ToDescriptionString());

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.GetLoginLinkCommandHandled, result);
        }


        [Fact]
        public async Task Update_AddTrackThatAlreadyExists_TrackAlreadyExistsReturned()
        {
            // Arrange.
            const string trackUrlThatAlreadyExists = "https://open.spotify.com/track/6HMvJcdw6qLsyV1b5x29sa";

            using var stream = new MemoryStream();
            var httpRequest = await CreateRequest(stream, trackUrlThatAlreadyExists);

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            AssertHelper.Equal(BotResponseCode.TrackAlreadyExists, result);
        }

        [Fact]
        public async Task Update_AddTrack_TrackAddedToPlaylistReturned()
        {
            // Arrange.
            const string trackId = "5R3eXsNtC8w3eVBT3RsUIa";
            const string trackUrl = "https://open.spotify.com/track/5R3eXsNtC8w3eVBT3RsUIa";

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
                // Make sure the track is deleted after the test is done.
                await _spotifyClientService.RemoveTrackFromPlaylist(trackId);
                
                // Remove the track from storage.
                var track = await _trackRepository.Get(trackId);
                await _trackRepository.Delete(track);
            }
        }

        [Fact]
        public async Task Update_UpvoteCallbackTwice_UpvoteHandledReturned_DownvoteHandledReturned()
        {
            // Arrange.
            const string trackUrl = "https://open.spotify.com/track/5R3eXsNtC8w3eVBT3RsUIa";

            // Send the two message that go before an upvote callback: one with a trackUrl and a reply from the bot with an upvote button.
            var trackMessageId = await _sendMessageService.SendTextMessageAsync(_testOptions.TestChatId, trackUrl);
            var botReplyMessageId = await _sendMessageService.SendTextMessageAsync(_testOptions.TestChatId, "Track added to playlist!", replyToMessageId: trackMessageId);

            // Send two callback updates, first to test an upvote...
            using (var stream = new MemoryStream())
            {
                var httpRequest = await CreateUpvoteRequest(stream, botReplyMessageId, trackUrl);

                // Act.
                var upvoteResult = await _sut.Run(httpRequest);


                // Assert.
                AssertHelper.Equal(BotResponseCode.UpvoteHandled, upvoteResult);
            }

            // ...and then test a downvote.
            using (var stream = new MemoryStream())
            {
                var httpRequest = await CreateUpvoteRequest(stream, botReplyMessageId, trackUrl);

                // Act.
                var downvoteResult = await _sut.Run(httpRequest);

                // Assert.
                AssertHelper.Equal(BotResponseCode.DownvoteHandled, downvoteResult);
            }
        }

        private async Task<HttpRequest> CreateRequest(Stream stream, string textMessage)
        {
            await _generateUpdateStreamService.WriteTextMessageToStream(stream, textMessage);

            return CreateRequestWithBody(stream);
        }

        private async Task<HttpRequest> CreateUpvoteRequest(Stream stream, int messageId, string trackUrl)
        {
            await _generateUpdateStreamService.WriteUpvoteCallbackQueryToStream(stream, messageId, trackUrl);

            return CreateRequestWithBody(stream);
        }

        private HttpRequest CreateRequestWithBody(Stream bodyStream)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = bodyStream;

            return httpContext.Request;
        }
    }
}
