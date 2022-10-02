using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using SpotiBot.Library.Exceptions;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using SpotiBot.Bot.WebApp;
using SpotiBot.Spotify.Authorization;
using IMapper = SpotiBot.Bot.WebApp.IMapper;
using System.Threading.Tasks;
using SpotiBot.Bot.WebApp.Models;
using SpotiApiModels;

namespace SpotiBot
{
    public class InitData
    {
        private readonly Library.Options.SentryOptions _sentryOptions;
        private readonly IWebAppValidationService _webAppValidationService;
        private readonly IAuthorizationTokenRepository _authorizationTokenRepository;
        private readonly IMapper _webAppMapper;
        private readonly ApiModels.IMapper _apiMapper;

        public InitData(
            IOptions<Library.Options.SentryOptions> sentryOptions,
            IWebAppValidationService webAppValidationService,
            IAuthorizationTokenRepository authorizationTokenRepository,
            IMapper webAppMapper,
            ApiModels.IMapper apiMapper)
        {
            _sentryOptions = sentryOptions.Value;
            _webAppValidationService = webAppValidationService;
            _authorizationTokenRepository = authorizationTokenRepository;
            _webAppMapper = webAppMapper;
            _apiMapper = apiMapper;
        }

        [FunctionName(nameof(InitData))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    if (!_webAppValidationService.IsRequestFromTelegram(httpRequest.Query))
                        return new BadRequestResult();

                    var webAppInitData = _webAppMapper.Map(httpRequest.Query);

                    if (IsExpired(webAppInitData))
                        return new BadRequestResult();

                    // When telegram did not send us a user, we're done here.
                    if (webAppInitData.User == null)
                        return new NoContentResult();

                    var token = await _authorizationTokenRepository.Get(webAppInitData.User.Id);

                    // If we cannot find a token, we're done here.
                    if (token == null)
                        return new NoContentResult();
                    
                    return new OkObjectResult(new InitDataResult(_apiMapper.Map(token)));
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);
                    throw;
                }
            }
        }

        private static bool IsExpired(WebAppInitData webAppInitData)
        {
            // We only handle requests that were sent in the last few minutes.
            return webAppInitData.AuthDate < DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();
        }
    }
}
