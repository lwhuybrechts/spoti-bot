using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Spoti_bot.Bot;
using Spoti_bot.Bot.Chats;
using Spoti_bot.IntegrationTests.Library;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify.Api;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Tracks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Spoti_bot.IntegrationTests
{
    public class CallbackTests : IClassFixture<TestHost>
    {
        private readonly Callback _sut;

        private readonly TestOptions _testOptions;

        private readonly ILoginRequestRepository _loginRequestRepository;

        public CallbackTests(TestHost testHost)
        {
            _testOptions = testHost.GetService<IOptions<TestOptions>>().Value;

            _loginRequestRepository = testHost.GetService<ILoginRequestRepository>();

            var spotifyAuthorizationService = testHost.GetService<IAuthorizationService>();
            var sendMessageService = testHost.GetService<ISendMessageService>();
            var chatRepository = testHost.GetService<IChatRepository>();
            var trackRepository = testHost.GetService<ITrackRepository>();
            var sentryOptions = testHost.GetService<IOptions<SentryOptions>>();
            var spotifyClientFactory = testHost.GetService<ISpotifyClientFactory>();
            var spotifyClientService = testHost.GetService<ISpotifyClientService>();

            _sut = new Callback(spotifyAuthorizationService, sendMessageService, chatRepository, trackRepository, spotifyClientFactory, spotifyClientService, sentryOptions);
        }

        private async Task TruncateTables()
        {
            // TODO: use dev tables and do actual truncates.
            // Remove specific deletes in these tests since the tables are truncated.
            await DeleteLoginRequest();
        }

        [Fact]
        public async Task Run_NoCodeInQuery_ErrorMessageReturned()
        {
            // Arrange.
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "state", "1234" }
            });

            // Act.
            var result = await _sut.Run(httpContext.Request);

            // Assert.
            AssertHelper.Equal(Callback.ErrorMessage, result);
        }

        [Fact]
        public async Task Run_NoLoginRequestIdInQuery_ErrorMessageReturned()
        {
            // Arrange.
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "code", "1234" }
            });

            // Act.
            var result = await _sut.Run(httpContext.Request);

            // Assert.
            AssertHelper.Equal(Callback.ErrorMessage, result);
        }

        [Fact(Skip = "TODO: add a valid code so the callback can be tested.")]
        public async Task Run_ValidCodeInQuery_SuccessMessageReturned()
        {
            // Arrange.
            await TruncateTables();

            var loginRequest = await InsertLoginRequest();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                // TODO: how to get a valid code?
                { "code", "1234" },
                { "state", loginRequest.Id }
            });

            // Act.
            var result = await _sut.Run(httpContext.Request);

            // Assert.
            AssertHelper.Equal(Callback.SuccessMessage, result);
        }

        private Task<LoginRequest> InsertLoginRequest()
        {
            return _loginRequestRepository.Upsert(CreateLoginRequest());
        }

        private async Task DeleteLoginRequest()
        {
            var loginRequest = await _loginRequestRepository.Get(CreateLoginRequest());

            if (loginRequest != null)
                await _loginRequestRepository.Delete(loginRequest);
        }

        private LoginRequest CreateLoginRequest()
        {
            return new LoginRequest
            {
                UserId = _testOptions.TestUserId,
                // Make sure the same LoginRequest is used in all tests.
                Id = "ab6025ef-e707-46bc-b06f-7b30f7b1fd1e",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                PrivateChatId = _testOptions.PrivateTestChatId
            };
        }
    }
}
