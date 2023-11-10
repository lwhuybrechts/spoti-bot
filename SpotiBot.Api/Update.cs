using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System;
using Sentry;
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SpotiBot.Library.Enums;
using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Bot.HandleUpdate.Commands;
using SpotiBot.Api.Bot.HandleUpdate;
using SpotiBot.Api.Library;
using SpotiBot.Api.Library.Exceptions;

namespace SpotiBot.Api
{
    public class Update
    {
        private readonly IHandleMessageService _handleMessageService;
        private readonly IHandleCallbackQueryService _handleCallbackQueryService;
        private readonly IHandleInlineQueryService _handleInlineQueryService;
        private readonly ICommandsService _commandsService;
        private readonly IUpdateDtoService _updateDtoService;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Update(
            IHandleMessageService handleMessageService,
            IHandleCallbackQueryService handleCallbackQueryService,
            IHandleInlineQueryService handleInlineQueryService,
            ICommandsService commandsService,
            IUpdateDtoService updateDtoService,
            IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _handleMessageService = handleMessageService;
            _handleCallbackQueryService = handleCallbackQueryService;
            _handleInlineQueryService = handleInlineQueryService;
            _commandsService = commandsService;
            _updateDtoService = updateDtoService;
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(Update))]
        public async Task<IStatusCodeActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest httpRequest)
        {
            var requestBody = await GetRequestBody(httpRequest);

            // Setup exception handling, add to request body so we have all the info we need.
            using (new SentryExceptionHandler(_sentryOptions.Dsn, requestBody))
            {
                try
                {
                    var update = JsonConvert.DeserializeObject<Telegram.Bot.Types.Update>(requestBody);
                    var updateDto = await _updateDtoService.Build(update);

                    // Only handle updates on certain conditions.
                    if (!ShouldHandle(updateDto))
                        return new OkObjectResult(BotResponseCode.NoAction);

                    // Check if we can do something with the text message.
                    var messageResponseCode = await _handleMessageService.TryHandleMessage(updateDto);
                    if (messageResponseCode != BotResponseCode.NoAction)
                        return new OkObjectResult(messageResponseCode);

                    // Check if we can do something with the callback query.
                    // Callback queries are sent when the user clicks a button.
                    var callbackQueryResponseCode = await _handleCallbackQueryService.TryHandleCallbackQuery(updateDto);
                    if (callbackQueryResponseCode != BotResponseCode.NoAction)
                        return new OkObjectResult(callbackQueryResponseCode);

                    // Check if we can do something with the inline query.
                    // Inline queries are sent when the user types a query behind the bot username tag: @Spotibot query
                    var inlineQueryResponseCode = await _handleInlineQueryService.TryHandleInlineQuery(updateDto);
                    if (inlineQueryResponseCode != BotResponseCode.NoAction)
                        return new OkObjectResult(inlineQueryResponseCode);

                    return new OkObjectResult(BotResponseCode.NoAction);
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);

                    // Don't send a message to the chat, since it can create a lot of spam.

                    return new OkObjectResult(BotResponseCode.ExceptionHandled);
                }
            }
        }

        private static async Task<string> GetRequestBody(HttpRequest httpRequest)
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

        private bool ShouldHandle(UpdateDto updateDto)
        {
            if (updateDto == null)
                return false;

            // Always handle inline queries, since they do not have chat info.
            if (updateDto.ParsedUpdateType == UpdateType.InlineQuery)
                return true;

            var chatType = updateDto.ParsedChat?.Type;

            if (!chatType.HasValue)
                return false;

            switch (chatType.Value)
            {
                case ChatType.Group:
                case ChatType.Supergroup:
                    return true;
                case ChatType.Private:
                    // The only thing the bot handles in a private chat is one of the following commands.
                    return IsStartCommand(updateDto) ||
                        IsGetLoginLinkCommand(updateDto) ||
                        IsWebAppCommand(updateDto);
                case ChatType.Channel:
                default:
                    return false;
            }
        }

        private bool IsStartCommand(UpdateDto updateDto)
        {
            return IsCommand(updateDto, Command.Start);
        }

        private bool IsGetLoginLinkCommand(UpdateDto updateDto)
        {
            return IsCommand(updateDto, Command.GetLoginLink);
        }

        private bool IsWebAppCommand(UpdateDto updateDto)
        {
            return IsCommand(updateDto, Command.WebApp);
        }

        private bool IsCommand(UpdateDto updateDto, Command command)
        {
            return _commandsService.IsCommand(updateDto.ParsedTextMessage, command);
        }
    }
}