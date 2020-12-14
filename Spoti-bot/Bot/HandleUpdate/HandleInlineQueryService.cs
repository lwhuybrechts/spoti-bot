using AutoMapper;
using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify;
using System;
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
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly IMapper _mapper;

        public HandleInlineQueryService(
            ISendMessageService sendMessageService,
            ICommandsService commandsService,
            IUserService userService,
            ISpotifyLinkHelper spotifyLinkHelper,
            IMapper mapper)
        {
            _sendMessageService = sendMessageService;
            _commandsService = commandsService;
            _userService = userService;
            _spotifyLinkHelper = spotifyLinkHelper;
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

            if (_commandsService.IsCommand(update.InlineQuery.Query, InlineQueryCommand.GetUpvoteUsers))
            {
                // The query should be a trackId.
                var trackId = _commandsService.GetQuery(update.InlineQuery.Query, InlineQueryCommand.GetUpvoteUsers);

                if (string.IsNullOrEmpty(trackId))
                    return BotResponseCode.NoAction;

                // TODO: only get upvotes from the current chat/playlist.
                var users = await _userService.GetUpvoteUsers(trackId);

                // TODO: add user images.

                // Send the results back.
                var inlineQueryResults = _mapper.Map<List<InlineQueryResultArticle>>(users);

                if (inlineQueryResults.Any())
                {
                    // TODO: hide image for this row.
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

            if (_commandsService.IsAnyCommand<InlineQueryCommand>(update.InlineQuery.Query))
                return true;

            return false;
        }

        private async Task AnswerInlineQuery(Telegram.Bot.Types.Update update, IEnumerable<InlineQueryResultBase> results)
        {
            try
            {
                await _sendMessageService.AnswerInlineQuery(update.InlineQuery.Id, results);
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
