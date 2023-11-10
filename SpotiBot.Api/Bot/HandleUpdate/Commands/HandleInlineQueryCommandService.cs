using SpotiBot.Data.Services;
using SpotiBot.Library.Enums;
using SpotiBot.Library.BusinessModels.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineQueryResults;
using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using SpotiBot.Api.Spotify;

namespace SpotiBot.Api.Bot.HandleUpdate.Commands
{
    public class HandleInlineQueryCommandService : BaseCommandsService<InlineQueryCommand>, IHandleInlineQueryCommandService
    {
        private readonly ICommandsService _commandsService;
        private readonly IUserService _userService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IVoteService _voteService;

        public HandleInlineQueryCommandService(
            ICommandsService commandsService,
            IUserService userService,
            ISendMessageService sendMessageService,
            IVoteService voteService,
            ISpotifyLinkHelper spotifyLinkHelper)
            : base(commandsService, userService, sendMessageService, spotifyLinkHelper)
        {
            _commandsService = commandsService;
            _userService = userService;
            _sendMessageService = sendMessageService;
            _voteService = voteService;
        }

        protected override Task<BotResponseCode> HandleCommand(InlineQueryCommand command, UpdateDto updateDto)
        {
            return command switch
            {
                InlineQueryCommand.Connect => HandleConnectInPrivateChatLink(updateDto),
                InlineQueryCommand.GetVoteUsers => HandleGetVoteUsers(updateDto),
                _ => throw new NotImplementedException($"InlineQueryCommand {command} has no handle function defined.")
            };
        }

        private async Task<BotResponseCode> HandleConnectInPrivateChatLink(UpdateDto updateDto)
        {
            var groupChatId = _commandsService.GetQuery(updateDto.ParsedTextMessage, InlineQueryCommand.Connect);

            if (string.IsNullOrEmpty(groupChatId))
                return BotResponseCode.CommandRequirementNotFulfilled;

            var switchPmText = "Connect in private chat";
            var switchPmParameter = $"{LoginRequestReason.AddBotToGroupChat}_{groupChatId}";

            await _sendMessageService.AnswerInlineQuery(updateDto.ParsedUpdateId, new List<InlineQueryResultArticle>(), switchPmText, switchPmParameter);

            return BotResponseCode.InlineQueryHandled;
        }

        private async Task<BotResponseCode> HandleGetVoteUsers(UpdateDto updateDto)
        {
            var (playlistId, trackId) = GetUpvoteUsersQueries(updateDto);

            if (string.IsNullOrEmpty(playlistId) || string.IsNullOrEmpty(trackId))
                return BotResponseCode.CommandRequirementNotFulfilled;

            var votes = await _voteService.Get(playlistId, trackId);
            var users = await GetUsersForVotes(votes);

            var results = CreateVoteUsersResults(votes, users);

            // Send the results to the chat.
            await _sendMessageService.AnswerInlineQuery(updateDto.ParsedUpdateId, results);

            return BotResponseCode.InlineQueryHandled;
        }

        private (string, string) GetUpvoteUsersQueries(UpdateDto updateDto)
        {
            var queries = _commandsService.GetQueries(updateDto.ParsedTextMessage, InlineQueryCommand.GetVoteUsers);

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

        private Task<List<User>> GetUsersForVotes(List<Vote> votes)
        {
            if (!votes.Any())
                return Task.FromResult(new List<User>());

            var userIds = votes.Select(x => x.UserId).Distinct().ToList();

            return _userService.Get(userIds);
        }

        private IEnumerable<InlineQueryResultArticle> CreateVoteUsersResults(List<Vote> votes, List<User> users)
        {
            var results = new List<InlineQueryResultArticle>();

            // Add the users that voted per VoteType.
            foreach (var voteType in Enum.GetValues(typeof(VoteType)).Cast<VoteType>())
            {
                var voteTypeArticles = GetArticles(voteType, votes, users);

                if (voteTypeArticles.Any())
                    results.AddRange(voteTypeArticles);
            }

            return results;
        }

        private static List<InlineQueryResultArticle> GetArticles(VoteType voteType, List<Vote> votes, List<User> users)
        {
            var voteTypeVotes = votes.Where(x => x.Type == voteType).ToList();

            if (!voteTypeVotes.Any())
                return new List<InlineQueryResultArticle>();

            var voteTypeUserIds = voteTypeVotes.Select(x => x.UserId);

            var voteTypeUsers = users.Where(x => voteTypeUserIds.Contains(x.Id)).ToList();

            if (!voteTypeUsers.Any())
                return new List<InlineQueryResultArticle>();

            var inlineQueryResults = new List<InlineQueryResultArticle>
            {
                // Add a description as the first row.
                CreateArticle($"Users that gave this a track a {KeyboardService.GetVoteButtonText(voteType)}")
            };

            foreach (var voteTypeUser in voteTypeUsers)
                inlineQueryResults.Add(CreateArticle(voteTypeUser.FirstName));

            return inlineQueryResults;
        }

        private static InlineQueryResultArticle CreateArticle(string text)
        {
            return new InlineQueryResultArticle(Guid.NewGuid().ToString(), text, new InputTextMessageContent(text));
        }
    }
}
