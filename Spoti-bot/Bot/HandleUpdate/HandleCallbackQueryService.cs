using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Bot.Votes;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.HandleUpdate
{
    public class HandleCallbackQueryService : IHandleCallbackQueryService
    {
        private readonly IVoteService _voteService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IUserService _userService;

        public HandleCallbackQueryService(IVoteService voteService, ISendMessageService sendMessageService, IUserService userService)
        {
            _voteService = voteService;
            _sendMessageService = sendMessageService;
            _userService = userService;
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

                // Let telegram know the callback query has been handled.
                await AnswerCallback(updateDto, voteResponseCode);

                return voteResponseCode;
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
                updateDto?.Track == null)
                return false;

            if (!_voteService.IsAnyVoteCallback(updateDto))
                return false;

            return true;
        }

        /// <summary>
        /// Let telegram know the callback query has been handled.
        /// </summary>
        private Task AnswerCallback(UpdateDto updateDto, BotResponseCode botResponseCode)
        {
            string text = null;
            switch (botResponseCode)
            {
                case BotResponseCode.AddVoteHandled:
                    text = "Added vote";
                    break;
                case BotResponseCode.RemoveVoteHandled:
                    text = "Removed vote";
                    break;
            }

            return _sendMessageService.AnswerCallbackQuery(updateDto.ParsedUpdateId, text);
        }
    }
}
