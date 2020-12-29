using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Bot.Votes;
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
        private readonly IUserRepository _userRepository;
        private readonly ISendMessageService _sendMessageService;
        private readonly IVoteRepository _voteRepository;

        public HandleInlineQueryCommandService(
            ICommandsService commandsService,
            IUserRepository userRepository,
            ISendMessageService sendMessageService,
            IVoteRepository voteRepository,
            ISpotifyLinkHelper spotifyLinkHelper)
            : base(commandsService, userRepository, sendMessageService, spotifyLinkHelper)
        {
            _commandsService = commandsService;
            _userRepository = userRepository;
            _sendMessageService = sendMessageService;
            _voteRepository = voteRepository;
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
            var switchPmParameter = groupChatId;

            await _sendMessageService.AnswerInlineQuery(updateDto.ParsedUpdateId, new List<InlineQueryResultArticle>(), switchPmText, switchPmParameter);

            return BotResponseCode.InlineQueryHandled;
        }

        private async Task<BotResponseCode> HandleGetVoteUsers(UpdateDto updateDto)
        {
            var (playlistId, trackId) = GetUpvoteUsersQueries(updateDto);

            if (string.IsNullOrEmpty(playlistId) || string.IsNullOrEmpty(trackId))
                return BotResponseCode.CommandRequirementNotFulfilled;

            var votes = await _voteRepository.GetVotes(playlistId, trackId);

            List<User> users;
            if (!votes.Any())
                users = new List<User>();
            else
                // TODO: only get users that voted?
                users = await _userRepository.GetAll();

            var results = CreateResults(votes, users);

            // Send the results to the chat.
            await _sendMessageService.AnswerInlineQuery(updateDto.ParsedUpdateId, results);

            return BotResponseCode.InlineQueryHandled;
        }

        private IEnumerable<InlineQueryResultBase> CreateResults(List<Vote> votes, List<User> users)
        {
            // TODO: add user images.

            var results = new List<InlineQueryResultBase>();

            // Add the users that voted per VoteType.
            foreach (var voteType in Enum.GetValues(typeof(VoteType)).Cast<VoteType>())
            {
                var voteTypeVotes = votes.Where(x => x.Type == voteType).ToList();

                if (!voteTypeVotes.Any())
                    continue;

                var voteTypeUsers = users
                    .Where(x => voteTypeVotes
                        .Select(x => x.UserId)
                        .Contains(x.Id)
                    ).ToList();

                if (!voteTypeUsers.Any())
                    continue;

                // Add a description as the first row.
                var titleText = $"Users that gave this a track a {KeyboardService.GetVoteButtonText(voteType)}";
                var inlineQueryResults = new List<InlineQueryResultArticle>
                {
                    CreateArticle(titleText)
                };

                foreach (var voteTypeUser in voteTypeUsers)
                    inlineQueryResults.Add(CreateArticle(voteTypeUser.FirstName));
                
                results.AddRange(inlineQueryResults);
            }

            return results;
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

        private InlineQueryResultArticle CreateArticle(string text)
        {
            return new InlineQueryResultArticle(Guid.NewGuid().ToString(), text, new InputTextMessageContent(text));
        }
    }
}
