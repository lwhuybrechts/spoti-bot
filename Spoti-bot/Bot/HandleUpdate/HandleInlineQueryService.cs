using AutoMapper;
using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace Spoti_bot.Bot.HandleUpdate
{
    public class HandleInlineQueryService : IHandleInlineQueryService
    {
        private readonly ISendMessageService _sendMessageService;
        private readonly ICommandsService _commandsService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public HandleInlineQueryService(
            ISendMessageService sendMessageService,
            ICommandsService commandsService,
            IUserService userService,
            IMapper mapper)
        {
            _sendMessageService = sendMessageService;
            _commandsService = commandsService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<BotResponseCode> TryHandleInlineQuery(UpdateDto updateDto)
        {
            // If the bot can't do anything with the update's inline query, we're done.
            if (!CanHandleInlineQuery(updateDto.Update))
            {
                // If there is an id, let telegram know the inline query has been handled.
                if (!string.IsNullOrEmpty(updateDto.Update?.InlineQuery?.Id))
                    await AnswerInlineQuery(updateDto.Update.InlineQuery.Id, new List<InlineQueryResultArticle>());
                
                return BotResponseCode.NoAction;
            }

            if (_commandsService.IsCommand(updateDto.Update.InlineQuery.Query, InlineQueryCommand.GetUpvoteUsers))
            {
                var queries = _commandsService.GetQueries(updateDto.Update.InlineQuery.Query, InlineQueryCommand.GetUpvoteUsers);

                string playlistId;
                string trackId;

                if (queries.ElementAtOrDefault(2).Length > 0)
                {
                    // The query should have a playlistId and a trackId.
                    playlistId = queries.ElementAtOrDefault(1);
                    trackId = queries.ElementAtOrDefault(2);
                }
                else
                {
                    // TODO: remove.
                    // Added for backwards compatability for messages without a playlistId.
                    playlistId = "2tnyzyB8Ku9XywzAYNjLxj";
                    trackId = queries.ElementAtOrDefault(1);
                }

                if (string.IsNullOrEmpty(playlistId) || string.IsNullOrEmpty(trackId))
                    return BotResponseCode.NoAction;

                var users = await _userService.GetUpvoteUsers(playlistId, trackId);

                // TODO: add user images.

                // Send the results back.
                var inlineQueryResults = _mapper.Map<List<InlineQueryResultArticle>>(users);

                if (inlineQueryResults.Any())
                {
                    // TODO: hide image for this row.
                    var titleText = "This track is upvoted by:";
                    inlineQueryResults.Insert(0, new InlineQueryResultArticle("resultId", titleText, new InputTextMessageContent(titleText)));
                }

                await AnswerInlineQuery(updateDto.Update.InlineQuery.Id, inlineQueryResults);

                return BotResponseCode.InlineQueryHandled;
            }

            // This should never happen.
            throw new InlineQueryNotHandledException();
        }

        private bool CanHandleInlineQuery(Telegram.Bot.Types.Update update)
        {
            // Check if we have all the data we need.
            if (update == null ||
                // Filter everything but inline queries.
                update.Type != UpdateType.InlineQuery ||
                update.InlineQuery == null ||
                string.IsNullOrEmpty(update.InlineQuery.Id) ||
                update.InlineQuery.From == null ||
                string.IsNullOrEmpty(update.InlineQuery.Query))
                return false;

            if (_commandsService.IsAnyCommand<InlineQueryCommand>(update.InlineQuery.Query))
                return true;

            return false;
        }

        private async Task AnswerInlineQuery(string inlineQueryId, IEnumerable<InlineQueryResultBase> results)
        {
            try
            {
                await _sendMessageService.AnswerInlineQuery(inlineQueryId, results);
            }
            catch (InvalidParameterException exception)
            {
                // This may crash when the inline query is too old, just ignore it.
                if (exception?.Message == "query is too old and response timeout expired or query ID is invalid")
                    return;

                throw;
            }
        }
    }
}
