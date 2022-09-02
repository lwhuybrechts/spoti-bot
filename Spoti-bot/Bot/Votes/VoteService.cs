using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using Spoti_bot.Spotify.Tracks.RemoveTrack;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using static Spoti_bot.Bot.Votes.VoteAttributes;

namespace Spoti_bot.Bot.Votes
{
    public class VoteService : IVoteService
    {
        public const int RemoveTrackOnDownvoteCount = 3;

        private readonly ISendMessageService _sendMessageService;
        private readonly IKeyboardService _keyboardService;
        private readonly IVoteTextHelper _voteTextHelper;
        private readonly IVoteRepository _voteRepository;
        private readonly IRemoveTrackService _removeTrackService;

        public VoteService(
            ISendMessageService sendMessageService,
            IKeyboardService keyboardService,
            IVoteTextHelper voteTextHelper,
            IVoteRepository voteRepository,
            IRemoveTrackService removeTrackService)
        {
            _sendMessageService = sendMessageService;
            _keyboardService = keyboardService;
            _voteTextHelper = voteTextHelper;
            _voteRepository = voteRepository;
            _removeTrackService = removeTrackService;
        }

        /// <summary>
        /// Check if the update is a vote callback.
        /// </summary>
        public bool IsAnyVoteCallback(UpdateDto updateDto)
        {
            return GetVoteType(updateDto) != null;
        }

        /// <summary>
        /// Check if an update contains an vote callback and if so, handle it.
        /// </summary>
        /// <param name="updateDto">The update to handle.</param>
        public Task<BotResponseCode> TryHandleVote(UpdateDto updateDto)
        {
            if (!IsAnyVoteCallback(updateDto))
                return Task.FromResult(BotResponseCode.NoAction);

            return HandleVote(updateDto, GetVoteType(updateDto).Value);
        }

        /// <summary>
        /// Get the VoteType from the update.
        /// </summary>
        /// <returns>The VoteType, or null if no VoteType was found.</returns>
        private static VoteType? GetVoteType(UpdateDto updateDto)
        {
            foreach (var voteType in Enum.GetValues(typeof(VoteType)).Cast<VoteType>())
                if (IsVoteCallback(updateDto, voteType))
                    return voteType;

            return null;
        }

        /// <summary>
        /// Check if an update contains a Vote.
        /// </summary>
        /// <returns>True if the update is a Vote callback.</returns>
        private static bool IsVoteCallback(UpdateDto updateDto, VoteType voteType)
        {
            if (string.IsNullOrEmpty(updateDto.ParsedData))
                return false;

            return updateDto.ParsedData.Equals(KeyboardService.GetVoteButtonText(voteType));
        }

        /// <summary>
        /// Add a Vote, or if it already exists remove the Vote.
        /// </summary>
        private async Task<BotResponseCode> HandleVote(UpdateDto updateDto, VoteType voteType)
        {
            var newVote = new Vote
            {
                PlaylistId = updateDto.Track.PlaylistId,
                TrackId = updateDto.Track.Id,
                UserId = updateDto.ParsedUser.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                Type = voteType
            };

            // A track can only be voted on once, so check if it already exists.
            var existingVote = await _voteRepository.Get(newVote);

            return existingVote == null
                ? await AddVote(updateDto, newVote)
                : await RemoveVote(updateDto, existingVote);
        }

        /// <summary>
        /// Add a vote.
        /// </summary>
        private async Task<BotResponseCode> AddVote(UpdateDto updateDto, Vote vote)
        {
            var responseCode = BotResponseCode.AddVoteHandled;

            // Save the vote in storage.
            await _voteRepository.Upsert(vote);

            var votes = await _voteRepository.GetVotes(updateDto.Track.PlaylistId, updateDto.Track.Id);

            string newText = null;
            InlineKeyboardMarkup keyboard = null;
            if (vote.Type == VoteType.Downvote)
            {
                var downVotes = votes.Where(x => x.Type == VoteType.Downvote).ToList();

                // If there are enough downvotes, remove the track.
                if (downVotes.Count >= RemoveTrackOnDownvoteCount)
                {
                    responseCode = await _removeTrackService.TryRemoveTrackFromPlaylist(updateDto);

                    newText = $"This track has been removed from the playlist because of {RemoveTrackOnDownvoteCount} downvotes.";

                    // Add only the See Votes button, so users can see who downvoted.
                    keyboard = _keyboardService.CreateSeeVotesKeyboard(updateDto.Track);
                }
            }

            if (string.IsNullOrEmpty(newText))
            {
                newText = UseNegativeOperator(vote)
                    // Increment or decrement the vote in the original message.
                    ? _voteTextHelper.DecrementVote(updateDto.ParsedTextMessageWithLinks, vote.Type)
                    : _voteTextHelper.IncrementVote(updateDto.ParsedTextMessageWithLinks, vote.Type);

                // If there are votes, add the See Votes button to the keyboard.
                keyboard = _keyboardService.AddOrRemoveSeeVotesButton(updateDto.ParsedInlineKeyboard, updateDto.Track, votes.Any());
            }
            
            await EditOriginalMessage(updateDto, newText, keyboard);

            // Let telegram know the callback query has been handled.
            await AnswerCallback(updateDto, $"Added {vote.Type}");

            return responseCode;
        }

        /// <summary>
        /// Remove a vote.
        /// </summary>
        private async Task<BotResponseCode> RemoveVote(UpdateDto updateDto, Vote existingVote)
        {
            // Delete the vote from storage.
            await _voteRepository.Delete(existingVote);

            var hasVotes = (await _voteRepository.GetVotes(updateDto.Track.PlaylistId, updateDto.Track.Id)).Any();

            var keyboard = _keyboardService.AddOrRemoveSeeVotesButton(updateDto.ParsedInlineKeyboard, updateDto.Track, hasVotes);

            var newText = UseNegativeOperator(existingVote)
                // Increment or decrement the vote in the original message.
                ? _voteTextHelper.IncrementVote(updateDto.ParsedTextMessageWithLinks, existingVote.Type)
                : _voteTextHelper.DecrementVote(updateDto.ParsedTextMessageWithLinks, existingVote.Type);

            await EditOriginalMessage(updateDto, newText, keyboard);

            // Let telegram know the callback query has been handled.
            await AnswerCallback(updateDto, $"Removed {existingVote.Type}");

            return BotResponseCode.RemoveVoteHandled;
        }

        /// <summary>
        /// Edit the message that triggered the callback query.
        /// </summary>
        /// <param name="updateDto">The update with the callback query.</param>
        /// <param name="newText">The new text to replace the message text with.</param>
        /// <param name="replyMarkup">The keyboard to use.</param>
        private Task EditOriginalMessage(UpdateDto updateDto, string newText, InlineKeyboardMarkup replyMarkup)
        {
            return _sendMessageService.EditMessageText(
                updateDto.Chat.Id,
                updateDto.ParsedBotMessageId.Value,
                newText,
                replyMarkup: replyMarkup);
        }

        /// <summary>
        /// Let telegram know the callback query has been handled.
        /// </summary>
        private Task AnswerCallback(UpdateDto updateDto, string text)
        {
            return _sendMessageService.AnswerCallbackQuery(updateDto.ParsedUpdateId, text);
        }

        private static bool UseNegativeOperator(Vote vote)
        {
            return vote.Type.HasAttribute<VoteType, UseNegativeOperatorAttribute>();
        }
    }
}