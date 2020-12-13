using AutoMapper;
using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace Spoti_bot.Bot.HandleUpdate
{
    public class HandleInlineQueryService : IHandleInlineQueryService
    {
        private readonly IInlineQueryCommandsService _inlineQueriesService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public HandleInlineQueryService(
            IInlineQueryCommandsService inlineQueriesService,
            ISendMessageService sendMessageService,
            IUserService userService,
            IMapper mapper)
        {
            _inlineQueriesService = inlineQueriesService;
            _sendMessageService = sendMessageService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<BotResponseCode> TryHandleInlineQuery(Telegram.Bot.Types.Update update)
        {
            // If the bot can't do anything with the update's inline query, we're done.
            if (!CanHandleInlineQuery(update))
            {
                if (!string.IsNullOrEmpty(update?.InlineQuery?.Id))
                    await AnswerInlineQuery(update, new List<InlineQueryResultArticle>());
                return BotResponseCode.NoAction;
            }

            if (_inlineQueriesService.IsCommand(update.InlineQuery.Query, InlineQueryCommand.GetUpvoteUsers))
            {
                // The query should be a trackId.
                var trackId = _inlineQueriesService.GetQuery(update.InlineQuery.Query, InlineQueryCommand.GetUpvoteUsers);

                if (string.IsNullOrEmpty(trackId))
                    return BotResponseCode.NoAction;

                var users = await _userService.GetUpvoteUsers(trackId);

                // Send the results back.
                var inlineQueryResults = _mapper.Map<List<InlineQueryResultArticle>>(users);

                if (inlineQueryResults.Any())
                {
                    var titleText = "This track is upvoted by:";
                    inlineQueryResults.Insert(0, new InlineQueryResultArticle("resultId", titleText, new InputTextMessageContent(titleText)));
                }

                await AnswerInlineQuery(update, inlineQueryResults);

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

            if (_inlineQueriesService.IsAnyCommand(update.InlineQuery.Query))
                return true;

            return false;
        }

        private Task AnswerInlineQuery(Telegram.Bot.Types.Update update, IEnumerable<InlineQueryResultBase> results)
        {
            return _sendMessageService.AnswerInlineQueryAsync(update.InlineQuery.Id, results);
        }
    }
}
