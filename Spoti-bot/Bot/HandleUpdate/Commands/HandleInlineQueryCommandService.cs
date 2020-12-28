using AutoMapper;
using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Spotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineQueryResults;

namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    public class HandleInlineQueryCommandService : BaseCommandsService<InlineQueryCommand>, IHandleInlineQueryCommandService
    {
        private readonly ICommandsService _commandsService;
        private readonly IUserService _userService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IMapper _mapper;

        public HandleInlineQueryCommandService(
            ICommandsService commandsService,
            IUserService userService,
            ISendMessageService sendMessageService,
            ISpotifyLinkHelper spotifyLinkHelper,
            IMapper mapper)
            : base(commandsService, userService, sendMessageService, spotifyLinkHelper)
        {
            _commandsService = commandsService;
            _userService = userService;
            _sendMessageService = sendMessageService;
            _mapper = mapper;
        }

        protected override Task<BotResponseCode> HandleCommand(InlineQueryCommand command, UpdateDto updateDto)
        {
            return command switch
            {
                InlineQueryCommand.GetUpvoteUsers => HandleGetUpvoteUsers(updateDto),
                _ => throw new NotImplementedException($"InlineQueryCommand {command} has no handle function defined.")
            };
        }

        private async Task<BotResponseCode> HandleGetUpvoteUsers(UpdateDto updateDto)
        {
            var (playlistId, trackId) = GetUpvoteUsersQueries(updateDto);

            if (string.IsNullOrEmpty(playlistId) || string.IsNullOrEmpty(trackId))
                return BotResponseCode.NoAction;

            var users = await _userService.GetUpvoteUsers(playlistId, trackId);

            var results = CreateResults(users);

            // Send the results to the chat.
            await _sendMessageService.AnswerInlineQuery(updateDto.ParsedUpdateId, results);

            return BotResponseCode.InlineQueryHandled;
        }

        private IEnumerable<InlineQueryResultBase> CreateResults(List<User> users)
        {
            // TODO: add user images.

            var inlineQueryResults = _mapper.Map<List<InlineQueryResultArticle>>(users);

            // Add a description as the first row.
            if (inlineQueryResults.Any())
            {
                // TODO: hide image for this row.
                var titleText = "This track is upvoted by:";
                inlineQueryResults.Insert(0, new InlineQueryResultArticle("resultId", titleText, new InputTextMessageContent(titleText)));
            }

            return inlineQueryResults;
        }

        private (string, string) GetUpvoteUsersQueries(UpdateDto updateDto)
        {
            var queries = _commandsService.GetQueries(updateDto.ParsedTextMessage, InlineQueryCommand.GetUpvoteUsers);

            string playlistId;
            string trackId;

            if (queries.ElementAtOrDefault(2)?.Length > 0)
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

            return (playlistId, trackId);
        }
    }
}
