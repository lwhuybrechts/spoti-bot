using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Spoti_bot.Library.Exceptions;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using AutoMapper;
using System.Collections.Generic;
using Spoti_bot.Bot.Upvotes;

namespace Spoti_bot
{
    public class Upvotes
    {
        private readonly IUpvoteRepository _upvoteRepository;
        private readonly IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Upvotes(IUpvoteRepository upvoteRepository, IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _upvoteRepository = upvoteRepository;
            _mapper = mapper;
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(Upvotes))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    // Get the upvotes and put them into the response message of the API.
                    var upvotes = await _upvoteRepository.GetAll();

                    // Map the upvotes to api models.
                    var apiUpvotes= _mapper.Map<List<ApiModels.Upvote>>(upvotes);

                    return new OkObjectResult(apiUpvotes);
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
