using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Spoti_bot.IntegrationTests.Library;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Spoti_bot.IntegrationTests
{
    public class CallbackTests : IClassFixture<TestHost>
    {
        private readonly Callback _sut;

        public CallbackTests(TestHost testHost)
        {
            var spotifyAuthorizationService = testHost.GetService<IAuthorizationService>();
            var sentryOptions = testHost.GetService<IOptions<SentryOptions>>();

            _sut = new Callback(spotifyAuthorizationService, sentryOptions);
        }

        [Fact]
        public async Task Callback_NoCodeInQuery_ErrorMessageReturned()
        {
            // Arrange.
            var httpContext = new DefaultHttpContext();

            // Act.
            var result = await _sut.Run(httpContext.Request);

            // Assert.
            AssertHelper.Equal(Callback.ErrorMessage, result);
        }

        [Fact]
        public async Task Callback_ValidCodeInQuery_SuccessMessageReturned()
        {
            // Arrange.
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                // TODO: how to get a valid code?
                { "code", "1234AB" }
            });

            // Act.
            var result = await _sut.Run(httpContext.Request);

            // Assert.
            AssertHelper.Equal(Callback.SuccessMessage, result);
        }
    }
}
