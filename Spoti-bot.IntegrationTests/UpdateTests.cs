using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Spoti_bot.Bot.Interfaces;
using Spoti_bot.IntegrationTests.Library;
using Spoti_bot.Library.Options;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Spoti_bot.IntegrationTests
{
    public class UpdateTests : IClassFixture<TestHost>
    {
        private readonly Update _sut;

        public UpdateTests(TestHost testHost)
        {
            var handleMessageService = testHost.GetService<IHandleMessageService>();
            var handleCallbackQueryService = testHost.GetService<IHandleCallbackQueryService>();
            var sentryOptions = testHost.GetService<IOptions<SentryOptions>>();

            _sut = new Update(handleMessageService, handleCallbackQueryService, sentryOptions);
        }

        [Fact]
        public async Task Test1Async()
        {
            // Arrange.
            var httpContext = new DefaultHttpContext();
            var httpRequest = httpContext.Request;

            // Act.
            var result = await _sut.Run(httpRequest);

            // Assert.
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode.Value);
        }
    }
}
