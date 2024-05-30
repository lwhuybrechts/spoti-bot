using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Sentry;
using SpotiBot.Bot.Votes;
using SpotiBot.Library.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpotiBot
{
    public class Upvotes
    {
        private readonly IVoteRepository _voteRepository;
        private readonly ApiModels.IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Upvotes(IVoteRepository voteRepository, ApiModels.IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _voteRepository = voteRepository;
            _mapper = mapper;
            _sentryOptions = sentryOptions.Value;
        }

        [Function(nameof(Upvotes))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    // For now, only return upvotes from the main playlist.
                    const string playlistId = "2tnyzyB8Ku9XywzAYNjLxj";

                    var votes = await _voteRepository.GetAllByPartitionKey(playlistId);

                    var upvotes = votes.Where(x => x.Type == VoteType.Upvote).ToList();

                    // Map the upvotes to api models.
                    var apiUpvotes = _mapper.Map(upvotes);

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
