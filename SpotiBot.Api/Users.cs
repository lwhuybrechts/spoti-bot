using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using SpotiBot.Data.Services;
using SpotiBot.Api.Library.Extensions;
using SpotiBot.Api.Library.Options;
using SpotiBot.Api.Library.Exceptions;

namespace SpotiBot.Api
{
    public class Users
    {
        private readonly IUserService _userService;
        private readonly SpotiBot.Library.ApiModels.IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;
        private readonly TestOptions _testOptions;

        public Users(IUserService userService, SpotiBot.Library.ApiModels.IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions, IOptions<TestOptions> testOptions)
        {
            _userService = userService;
            _mapper = mapper;
            _sentryOptions = sentryOptions.Value;
            _testOptions = testOptions.Value;
        }

        [FunctionName(nameof(Users))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    // Don't return the test user.
                    var users = await _userService.GetAllExcept(_testOptions.TestUserId);

                    // Map the users to api models.
                    var apiUsers = _mapper.Map(users);

                    httpRequest.AddResponseCaching(300);

                    return new OkObjectResult(apiUsers);
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);
                    throw;
                }
            }
        }
    }
}
