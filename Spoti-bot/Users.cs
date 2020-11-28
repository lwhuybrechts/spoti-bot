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

namespace Spoti_bot
{
    public class Users
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Users(IUserRepository userRepository, IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _sentryOptions = sentryOptions.Value;
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

                    // Map the users to api models.
                    var apiUsers = _mapper.Map<ApiModels.Upvote>(users);

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
