using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using SpotiBot.Api.Spotify.Tracks.RemoveTrack;
using SpotiBot.Library.BusinessModels.Bot;
using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpotiBot.Api.Bot.Votes
{
    public class VoteService : IVoteService
    {
        public const int RemoveTrackOnDownvoteCount = 3;

        private readonly ISendMessageService _sendMessageService;
        private readonly IKeyboardService _keyboardService;
        private readonly IVoteTextHelper _voteTextHelper;
        private readonly Data.Services.IVoteService _voteService;
        private readonly IRemoveTrackService _removeTrackService;

        public VoteService(
            ISendMessageService sendMessageService,
            IKeyboardService keyboardService,
            IVoteTextHelper voteTextHelper,
            Data.Services.IVoteService voteService,
            IRemoveTrackService removeTrackService)
        {
            _sendMessageService = sendMessageService;
            _keyboardService = keyboardService;
            _voteTextHelper = voteTextHelper;
            _voteService = voteService;
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
        /// Check if an update contains a vote callback and if so, handle it.
        /// </summary>
        /// <param name="updateDto">The update to handle.</param>
        public Task<BotResponseCode> TryHandleVote(UpdateDto updateDto)
        {
            var voteType = GetVoteType(updateDto);

            if (!voteType.HasValue)
                return Task.FromResult(BotResponseCode.NoAction);

            return HandleVote(updateDto, voteType.Value);
        }

        /// <summary>
        /// Update a text and keyboard with the current votes for a track.
        /// </summary>
        /// <param name="text">The text to add the votes to.</param>
        /// <param name="keyboard">The keyboard to add or remove the See Votes button from.</param>
        /// <param name="track">The track to show the votes for.</param>
        /// <returns>The updated text and keyboard.</returns>
        public async Task<(string, InlineKeyboardMarkup)> UpdateTextAndKeyboard(string text, InlineKeyboardMarkup keyboard, Track track)
        {
            var votes = await _voteService.Get(track.PlaylistId, track.Id);

            return UpdateTextAndKeyboard(text, keyboard, track, votes);
        }

        /// <summary>
        /// Update a text and keyboard with a list of votes.
        /// </summary>
        /// <param name="text">The text to add the votes to.</param>
        /// <param name="keyboard">The keyboard to add or remove the See Votes button from.</param>
        /// <param name="track">The track to show the votes for.</param>
        /// <param name="votes">The votes to update with.</param>
        /// <returns>The updated text and keyboard.</returns>
        private (string, InlineKeyboardMarkup) UpdateTextAndKeyboard(string text, InlineKeyboardMarkup keyboard, Track track, List<Vote> votes)
        {
            var newText = _voteTextHelper.ReplaceVotes(text, votes);

            // If there are votes, add the See Votes button to the keyboard.
            keyboard = _keyboardService.AddOrRemoveSeeVotesButton(keyboard, track, votes.Any());

            return (newText, keyboard);
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
            var newVote = new Vote(
                updateDto.Track.PlaylistId,
                DateTimeOffset.UtcNow,
                voteType,
                updateDto.Track.Id,
                updateDto.ParsedUser.Id
            );

            // A track can only be voted on once, so check if it already exists.
            var existingVote = await _voteService.Get(newVote);

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
            await _voteService.Upsert(vote);

            var votes = await _voteService.Get(updateDto.Track.PlaylistId, updateDto.Track.Id);

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
                (newText, keyboard) = UpdateTextAndKeyboard(updateDto.ParsedTextMessageWithLinks, updateDto.ParsedInlineKeyboard, updateDto.Track, votes);

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
            await _voteService.Delete(existingVote);

            var votes = await _voteService.Get(updateDto.Track.PlaylistId, updateDto.Track.Id);

            (var newText, var keyboard) = UpdateTextAndKeyboard(updateDto.ParsedTextMessageWithLinks, updateDto.ParsedInlineKeyboard, updateDto.Track, votes);

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
            // Don't edit the message if nothing changed.
            if (newText == updateDto.ParsedTextMessageWithLinks &&
                _keyboardService.AreSame(replyMarkup, updateDto.ParsedInlineKeyboard))
                return Task.CompletedTask;

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
    }
}