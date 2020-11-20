using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System;
using Sentry;
using Spoti_bot.Bot;
using System.IO;
using Microsoft.Extensions.Options;
using Spoti_bot.Library.Exceptions;
using Newtonsoft.Json;

namespace Spoti_bot
{
    public class Update
    {
        private readonly IHandleMessageService _handleMessageService;
        private readonly IHandleCallbackQueryService _handleCallbackQueryService;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Update(IHandleMessageService handleMessageService, IHandleCallbackQueryService handleCallbackQueryService, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _handleMessageService = handleMessageService;
            _handleCallbackQueryService = handleCallbackQueryService;
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(Update))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest httpRequest)
        {
            var requestBody = await GetRequestBody(httpRequest);

            // Setup exception handling, add to request body so we have all the info we need.
            using (new SentryExceptionHandler(_sentryOptions.Dsn, requestBody))
            {
                try
                {
                    var update = JsonConvert.DeserializeObject<Telegram.Bot.Types.Update>(requestBody);

                    // Check if we can do something with the text message.
                    if (await _handleMessageService.TryHandleMessage(update))
                        return new OkResult();

                    // CallbackQueries are responses to a message the bot has sent.
                    if (await _handleCallbackQueryService.TryHandleCallbackQuery(update))
                        return new OkResult();

                    return new OkResult();
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);
                    
                    // Don't send a message to the chat, since it can create a lot of spam.
                }
            }

            return new OkResult();
        }

        private async Task<string> GetRequestBody(HttpRequest httpRequest)
        {
            try
            {
                using var streamReader = new StreamReader(httpRequest.Body);
                return await streamReader.ReadToEndAsync();
            }
            catch (Exception)
            {
                // Catch this, since we don't have exception handling yet.
                return "";
            }
        }
    }
}
