using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Bot.Votes;
using Spoti_bot.Library;
using Spoti_bot.Spotify.Tracks;
using System;
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
        public const string DownvoteButtonText = "👎";
        private const string SpotiViewButtonText = "🔗";
        private readonly IVoteRepository _voteRepository;

        public KeyboardService(IVoteRepository voteRepository)
        {
            _voteRepository = voteRepository;
        }

        public InlineKeyboardMarkup CreateButtonKeyboard(string text, string url)
        {
            return new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl(text, url));
        }


        /// <summary>
        /// Create the keyboard that is added to the response the bot sends after a message with a spotify track.
        /// </summary>
        public InlineKeyboardMarkup CreatePostedTrackResponseKeyboard()
        {
            return new InlineKeyboardMarkup(new []
            {
                InlineKeyboardButton.WithCallbackData(UpvoteButtonText),
                InlineKeyboardButton.WithCallbackData(DownvoteButtonText),
                InlineKeyboardButton.WithUrl(SpotiViewButtonText, "https://skranenburg.outsystemscloud.com/SpotiView/")
            });
        }

        public static string GetVoteButtonText(VoteType voteType)
        {
            return voteType switch
            {
                VoteType.Upvote => UpvoteButtonText,
                VoteType.Downvote => DownvoteButtonText,
                _ => throw new NotImplementedException($"{nameof(VoteType)} {voteType} is not implemented in the {nameof(KeyboardService)}."),
            };
        }

        public async Task<InlineKeyboardMarkup> GetUpdatedVoteKeyboard(InlineKeyboardMarkup inlineKeyboard, Track track)
        {
            // Don't show the See Upvote button if there are no upvotes.
            if (!await HasUpvotes(track))
                return new InlineKeyboardMarkup(GetRowsWithoutSeeUpvoteButton(inlineKeyboard));

            // See if the See Upvote button already exists, if so keep the original keyboard.
            if (HasSeeUpvoteButton(inlineKeyboard))
                return new InlineKeyboardMarkup(inlineKeyboard.InlineKeyboard);

            // Add See Upvote button to the keyboard.
            return new InlineKeyboardMarkup(GetRowsWithSeeUpvoteButton(track, inlineKeyboard));
        }

        private static List<List<InlineKeyboardButton>> GetRowsWithSeeUpvoteButton(Track track, InlineKeyboardMarkup originalKeyboard)
        {
            var query = $"{InlineQueryCommand.GetUpvoteUsers.ToDescriptionString()} {track.PlaylistId} {track.Id}";

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
        private async Task<bool> HasUpvotes(Track track)
        {
            var votes = await _voteRepository.GetVotes(track.PlaylistId, track.Id);

            return votes.Where(x => x.Type == VoteType.Upvote).Any();
        }
    }
}