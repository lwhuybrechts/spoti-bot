using SpotiBot.Api.Bot.HandleUpdate.Commands;
using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using SpotiBot.Api.Library.Exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineQueryResults;

namespace SpotiBot.Api.Bot.HandleUpdate
{
    public class HandleInlineQueryService : IHandleInlineQueryService
    {
        private readonly ISendMessageService _sendMessageService;
        private readonly IHandleInlineQueryCommandService _inlineQueryCommandsService;

        public HandleInlineQueryService(
            ISendMessageService sendMessageService,
            IHandleInlineQueryCommandService inlineQueryCommandsService)
        {
            _sendMessageService = sendMessageService;
            _inlineQueryCommandsService = inlineQueryCommandsService;
        }

        public async Task<BotResponseCode> TryHandleInlineQuery(UpdateDto updateDto)
        {
            // If the bot can't do anything with the update's inline query, we're done.
            if (!CanHandleInlineQuery(updateDto))
            {
                // If there is an id, let telegram know the inline query has been handled.
                if (!string.IsNullOrEmpty(updateDto.ParsedUpdateId) &&
                    updateDto.ParsedUpdateId != "0")
                    await _sendMessageService.AnswerInlineQuery(updateDto.ParsedUpdateId, new List<InlineQueryResultArticle>());

                return BotResponseCode.NoAction;
            }

            // Check if any command should be handled and if so handle it.
            var commandResponseCode = await _inlineQueryCommandsService.TryHandleCommand(updateDto);
            if (commandResponseCode != BotResponseCode.NoAction)
                return commandResponseCode;

            // This should never happen.
            throw new InlineQueryNotHandledException();
        }

        private bool CanHandleInlineQuery(UpdateDto updateDto)
        {
            // Check if we have all the data we need.
            if (
                // Filter everything but inline queries.
                updateDto.ParsedUpdateType != UpdateType.InlineQuery ||
                string.IsNullOrEmpty(updateDto.ParsedUpdateId) ||
                updateDto.ParsedUser == null ||
                string.IsNullOrEmpty(updateDto.ParsedTextMessage))
                return false;

            if (_inlineQueryCommandsService.IsAnyCommand(updateDto.ParsedTextMessage))
                return true;

            return false;
        }
    }
}
