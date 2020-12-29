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
        private const string SeeVotesButtonTextLegacy = "See upvotes";
        private const string SeeVotesButtonText = "👥";
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


        public InlineKeyboardMarkup CreateSwitchToPmKeyboard(Chats.Chat chat)
        {
            var query = $"{InlineQueryCommand.Connect.ToDescriptionString()} {chat.Id}";
            
            return new InlineKeyboardMarkup(InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Let's go", query));
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
            // Don't show the See Votes button if there are no votes.
            if (!await HasVotes(track))
                return new InlineKeyboardMarkup(GetRowsWithoutSeeVotesButton(inlineKeyboard));

            // Check if the See Votes button already exists, if so keep the original keyboard.
            if (HasSeeVotesButton(inlineKeyboard))
                return new InlineKeyboardMarkup(inlineKeyboard.InlineKeyboard);

            // Add the See Votes button to the keyboard.
            return new InlineKeyboardMarkup(GetRowsWithSeeVotesButton(track, inlineKeyboard));
        }

        private static List<List<InlineKeyboardButton>> GetRowsWithSeeVotesButton(Track track, InlineKeyboardMarkup originalKeyboard)
        {
            var query = $"{InlineQueryCommand.GetVoteUsers.ToDescriptionString()} {track.PlaylistId} {track.Id}";

            // Create a button on the keybaord that starts an inline query with the GetVoteUsers command.
            var seeVotesButton = InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(SeeVotesButtonText, query);

            // Copy the original keyboard.
            var rows = new List<List<InlineKeyboardButton>>();
            foreach (var row in originalKeyboard.InlineKeyboard)
                rows.Add(row.ToList());

            var lastRow = rows.Last();
            var downvoteButton = lastRow.SingleOrDefault(x => x.CallbackData == DownvoteButtonText);

            if (downvoteButton != null)
                // Add the button after the downvote button.
                lastRow.Insert(lastRow.IndexOf(downvoteButton) + 1, seeVotesButton);
            else
                lastRow.Add(seeVotesButton);
            
            return rows;
        }

        /// <summary>
        /// Check if the See Votes button is on the keyboard.
        /// </summary>
        private bool HasSeeVotesButton(InlineKeyboardMarkup originalKeyboard)
        {
            var allButtons = originalKeyboard.InlineKeyboard.SelectMany(x => x).ToList();
            
            return allButtons.Any(x => x.Text == SeeVotesButtonText || x.Text == SeeVotesButtonTextLegacy);
        }

        /// <summary>
        /// Return all rows from the original keyboard, without the See Votes button.
        /// </summary>
        private static List<List<InlineKeyboardButton>> GetRowsWithoutSeeVotesButton(InlineKeyboardMarkup originalKeyboard)
        {
            var newRows = new List<List<InlineKeyboardButton>>();

            // Add everything but the See Votes button.
            foreach (var row in originalKeyboard.InlineKeyboard)
                newRows.Add(row.Where(x => x.Text != SeeVotesButtonText && x.Text != SeeVotesButtonTextLegacy).ToList());
            
            return newRows;
        }

        /// <summary>
        /// Check if there are votes for this track.
        /// </summary>
        private async Task<bool> HasVotes(Track track)
        {
            return (await _voteRepository.GetVotes(track.PlaylistId, track.Id)).Any();
        }
    }
}