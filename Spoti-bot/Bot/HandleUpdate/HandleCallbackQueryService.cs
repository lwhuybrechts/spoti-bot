using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Bot.Votes;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify.Api;
using Spoti_bot.Spotify.Authorization;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.HandleUpdate
{
    public class HandleCallbackQueryService : IHandleCallbackQueryService
    {
        private readonly IVoteService _voteService;
        private readonly IUserService _userService;
        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ISendMessageService _sendMessageService;

        public HandleCallbackQueryService(
            IVoteService voteService,
            IUserService userService,
            ISpotifyClientFactory spotifyClientFactory,
            ISpotifyClientService spotifyClientService,
            ISendMessageService sendMessageService)
        {
            _voteService = voteService;
            _userService = userService;
            _spotifyClientFactory = spotifyClientFactory;
            _spotifyClientService = spotifyClientService;
            _sendMessageService = sendMessageService;
        }

        public async Task<BotResponseCode> TryHandleCallbackQuery(UpdateDto updateDto)
        {
            // If the bot can't do anything with the update's callback query, we're done.
            if (!CanHandleCallbackQuery(updateDto))
                return BotResponseCode.NoAction;

            // Check if a vote should be handled and if so handle it.
            var voteResponseCode = await _voteService.TryHandleVote(updateDto);
            if (voteResponseCode != BotResponseCode.NoAction)
            {
                // Save users that voted.
                await _userService.SaveUser(updateDto.ParsedUser, updateDto.Chat.Id);

                return voteResponseCode;
            }

            if (IsAddToQueueCallback(updateDto))
            {
                if (updateDto.User == null ||
                    updateDto.Track == null)
                    return BotResponseCode.CommandRequirementNotFulfilled;

                var spotifyClient = await _spotifyClientFactory.Create(updateDto.User.Id);

                if (spotifyClient == null)
                {
                    var text = $"You didn't connect your Spotify account yet, {updateDto.User.FirstName}. Please connect first.";

                    // TODO: move to commandsservice, get username from telegram me apicall.
                    var url = $"http://t.me/SpotiHenkBot?start={LoginRequestReason.AddToQueue}_{updateDto.Chat.Id}_{updateDto.Track.Id}";

                    await _sendMessageService.AnswerCallbackQuery(updateDto.ParsedUpdateId, text, url);

                    return BotResponseCode.AddToQueueHandled;
                }

                if (await _spotifyClientService.AddToQueue(spotifyClient, updateDto.Track))
                    await _sendMessageService.AnswerCallbackQuery(updateDto.ParsedUpdateId, "Track added to your queue.");
                else
                    await _sendMessageService.AnswerCallbackQuery(updateDto.ParsedUpdateId, "Could not add track to your queue, play something first!");

                return BotResponseCode.AddToQueueHandled;
            }

            // This should never happen.
            throw new CallbackQueryNotHandledException();
        }

        /// <summary>
        /// Check if the bot can handle the update's callback query. 
        /// </summary>
        /// <returns>True if the bot can handle the callback query.</returns>
        private bool CanHandleCallbackQuery(UpdateDto updateDto)
        {
            // Check if we have all the data we need.
            if (
                // Filter everything but callback queries.
                updateDto.ParsedUpdateType != UpdateType.CallbackQuery ||
                string.IsNullOrEmpty(updateDto.ParsedUpdateId) ||
                updateDto.ParsedUser == null ||
                !updateDto.ParsedBotMessageId.HasValue ||
                string.IsNullOrEmpty(updateDto.ParsedTextMessage) ||
                string.IsNullOrEmpty(updateDto.ParsedTrackId) ||
                // Only handle callback queries in registered chats with a playlist.
                updateDto?.Chat == null ||
                updateDto?.Playlist == null ||
                updateDto?.Track == null ||
                // Removed tracks cannot be voted on, or added to the queue.
                updateDto?.Track?.State == Spotify.Tracks.TrackState.RemovedByDownvotes)
                return false;

            if (_voteService.IsAnyVoteCallback(updateDto))
                return true;

            if (IsAddToQueueCallback(updateDto))
                return true;

            return false;
        }

        private static bool IsAddToQueueCallback(UpdateDto updateDto)
        {
            return !string.IsNullOrEmpty(updateDto.ParsedData) &&
                (updateDto.ParsedData.Equals(KeyboardService.AddToQueueButtonText) ||
                 updateDto.ParsedData.Equals(KeyboardService.AddToQueueButtonTextLegacy));
        }
    }
}
