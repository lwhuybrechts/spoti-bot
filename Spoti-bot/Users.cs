using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using SpotiBot.Library.Exceptions;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using System.Linq;
using SpotiBot.Library.Options;
using SpotiBot.Bot.Users;
using SpotiBot.Library.Extensions;

namespace SpotiBot
{
    public class Users
    {
        private readonly IUserRepository _userRepository;
        private readonly ApiModels.IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;
        private readonly TestOptions _testOptions;

        public Users(IUserRepository userRepository, ApiModels.IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions, IOptions<TestOptions> testOptions)
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
                    users = users.Where(x => x.Id != _testOptions.TestUserId).ToList();

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
