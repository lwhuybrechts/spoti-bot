using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Spoti_bot.Library.Exceptions;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using Spoti_bot.Bot.Data.Users;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Spoti_bot.Library.Options;

namespace Spoti_bot
{
    public class Users
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;
        private readonly TestOptions _testOptions;

        public Users(IUserRepository userRepository, IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions, IOptions<TestOptions> testOptions)
        {
            _userRepository = userRepository;
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
                    // Get the users and put them into the response message of the API.
                    var users = await _userRepository.GetAll();

                    // Don't return the test user.
                    users = users.Where(x => x.Id != _testOptions.TestUserId.ToString()).ToList();

                    // Map the users to api models.
                    var apiUsers = _mapper.Map<List<ApiModels.User>>(users);

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
