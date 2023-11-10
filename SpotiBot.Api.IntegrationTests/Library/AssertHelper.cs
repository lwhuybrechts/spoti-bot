using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SpotiBot.Api.Library;
using System.Net;
using Xunit;

namespace SpotiBot.Api.IntegrationTests.Library
{
    public static class AssertHelper
    {
        public static void Equal(BotResponseCode expectedBotResponseCode, IStatusCodeActionResult result)
        {
            AssertStatusCode(result);

            // Check if the value is the expected response code.
            Assert.Equal(expectedBotResponseCode, (BotResponseCode)((OkObjectResult)result).Value);
        }

        public static void Equal(string expectedResult, IStatusCodeActionResult result)
        {
            AssertStatusCode(result);

            // Check if the value is the expected result string.
            Assert.Equal(expectedResult, ((OkObjectResult)result).Value);
        }

        private static void AssertStatusCode(IStatusCodeActionResult result)
        {
            // Check if it is a success statusCode.
            Assert.NotNull(result);
            Assert.NotNull(result?.StatusCode);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode.Value);
        }
    }
}
