using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Bot.Upvotes;
using Spoti_bot.Library;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot
{
    public class KeyboardService : IKeyboardService
    {
        private const string SeeUpvoteButtonTextLegacy = "See upvotes";
        private const string SeeUpvoteButtonText = "👥";
        public const string UpvoteButtonText = "👍";
        private const string SpotiViewButtonText = "🔗";
        private readonly IUpvoteRepository _upvoteRepository;

        public KeyboardService(IUpvoteRepository upvoteRepository)
        {
            _upvoteRepository = upvoteRepository;
        }

        public InlineKeyboardMarkup CreateKeyboard()
        {
            return new InlineKeyboardMarkup(new []
            {
                InlineKeyboardButton.WithCallbackData(UpvoteButtonText),
                InlineKeyboardButton.WithUrl(SpotiViewButtonText, "https://skranenburg.outsystemscloud.com/SpotiView/")
            });
        }

        public async Task<InlineKeyboardMarkup> GetUpdatedKeyboard(Message message, string trackId)
        {
            var originalKeyboard = message.ReplyMarkup;

            // Don't show See Upvote if there are no upvotes.
            if (!await HasUpvotes(trackId))
                return new InlineKeyboardMarkup(GetRowsWithoutSeeUpvoteButton(originalKeyboard));

            // See if Upvote button already exists, if so keep the original keyboard.
            if (HasSeeUpvoteButton(originalKeyboard))
                return new InlineKeyboardMarkup(originalKeyboard.InlineKeyboard);

            // Add See Upvote button to the keyboard.
            return new InlineKeyboardMarkup(GetRowsWithSeeUpvoteButton(trackId, originalKeyboard));
        }

        private static List<List<InlineKeyboardButton>> GetRowsWithSeeUpvoteButton(string trackId, InlineKeyboardMarkup originalKeyboard)
        {
            var query = $"{InlineQueryCommand.GetUpvoteUsers.ToDescriptionString()} {trackId}";

            // Create a button on the keybaord that starts an inline query with the GetUpvoteUsers command.
            var seeUpvotesButton = InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(SeeUpvoteButtonText, query);

            // Copy the original keyboard.
            var rows = new List<List<InlineKeyboardButton>>();
            foreach (var row in originalKeyboard.InlineKeyboard)
                rows.Add(row.ToList());

            var lastRow = rows.Last();
            var upvoteButton = lastRow.SingleOrDefault(x => x.CallbackData == UpvoteButtonText);

            if (upvoteButton != null)
                // Add the button after the upvote button.
                lastRow.Insert(lastRow.IndexOf(upvoteButton) + 1, seeUpvotesButton);
            else
                lastRow.Add(seeUpvotesButton);
            
            return rows;
        }

        /// <summary>
        /// Check if the See Upvote button is on the keyboard.
        /// </summary>
        private bool HasSeeUpvoteButton(InlineKeyboardMarkup originalKeyboard)
        {
            var allButtons = originalKeyboard.InlineKeyboard.SelectMany(x => x).ToList();
            
            return allButtons.Any(x => x.Text == SeeUpvoteButtonText || x.Text == SeeUpvoteButtonTextLegacy);
        }

        /// <summary>
        /// Return all rows from the original keyboard, without the See Upvote button.
        /// </summary>
        private static List<List<InlineKeyboardButton>> GetRowsWithoutSeeUpvoteButton(InlineKeyboardMarkup originalKeyboard)
        {
            var newRows = new List<List<InlineKeyboardButton>>();

            // Add everything but the See Upvote button.
            foreach (var row in originalKeyboard.InlineKeyboard)
                newRows.Add(row.Where(x => x.Text != SeeUpvoteButtonText && x.Text != SeeUpvoteButtonTextLegacy).ToList());
            
            return newRows;
        }

        /// <summary>
        /// Check if there are upvotes for this track.
        /// </summary>
        private async Task<bool> HasUpvotes(string trackId)
        {
            // TODO: fetch 1 record only.
            return (await _upvoteRepository.GetPartition(trackId)).Any();
        }
    }
}
