using Microsoft.Extensions.Options;
using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Bot.Votes;
using Spoti_bot.Library;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot
{
    public class KeyboardService : IKeyboardService
    {
        public const string UpvoteButtonText = "👍";
        public const string DownvoteButtonText = "👎";
        public const string AddToQueueButtonText = "➕";
        public const string AddToQueueButtonTextLegacy = "🕒";

        private const string SeeVotesButtonTextLegacy = "See upvotes";
        private const string SeeVotesButtonText = "👥";
        private const string SpotiViewButtonText = "🔗";
        private readonly IVoteRepository _voteRepository;
        private readonly AzureOptions _azureOptions;

        public KeyboardService(IVoteRepository voteRepository, IOptions<AzureOptions> azureOptions)
        {
            _voteRepository = voteRepository;
            _azureOptions = azureOptions.Value;
        }

        /// <summary>
        /// Create a keyboard with a single button, that opens an url when clicked.
        /// </summary>
        public InlineKeyboardMarkup CreateUrlKeyboard(string text, string url)
        {
            return new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl(text, url));
        }


        /// <summary>
        /// Create a keyboard with a single button, that opens a private chat with the user when clicked.
        /// </summary>
        public InlineKeyboardMarkup CreateSwitchToPmKeyboard(Chats.Chat chat)
        {
            var query = $"{InlineQueryCommand.Connect.ToDescriptionString()} {chat.Id}";

            return new InlineKeyboardMarkup(InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Let's go", query));
        }

        /// <summary>
        /// Create a keyboard that is added when a track is posted.
        /// </summary>
        public InlineKeyboardMarkup CreatePostedTrackResponseKeyboard()
        {
            // Add all vote buttons.
            var buttons = new List<InlineKeyboardButton>();
            foreach (var voteType in Enum.GetValues(typeof(VoteType)).Cast<VoteType>())
                buttons.Add(InlineKeyboardButton.WithCallbackData(GetVoteButtonText(voteType)));

            // TODO: move url to a setting.

            // Add a Add to Queue button.
            buttons.Add(InlineKeyboardButton.WithCallbackData(AddToQueueButtonText));

            // Add a button with a link to the front-end.
            buttons.Add(InlineKeyboardButton.WithUrl(SpotiViewButtonText, "https://skranenburg.outsystemscloud.com/SpotiView/"));

            return new InlineKeyboardMarkup(buttons);
        }

        /// <summary>
        /// Create a keyboard with a single button, that shows the votes of a track when clicked.
        /// </summary>
        public InlineKeyboardMarkup CreateSeeVotesKeyboard(Track track)
        {
            return new InlineKeyboardMarkup(AddSeeVotesButton(track));
        }

        public InlineKeyboardMarkup AddWebAppKeyboard()
        {
            return new InlineKeyboardMarkup(InlineKeyboardButton.WithWebApp(SpotiViewButtonText, new WebAppInfo
            {
                Url = GetWebAppUri().ToString()
            }));
        }

        /// <summary>
        /// Update the current keyboard according to if there are votes for a track.
        /// </summary>
        public InlineKeyboardMarkup AddOrRemoveSeeVotesButton(InlineKeyboardMarkup inlineKeyboard, Track track, bool hasVotes)
        {
            // Don't show the See Votes button if there are no votes.
            if (!hasVotes)
                return new InlineKeyboardMarkup(GetRowsWithoutSeeVotesButton(inlineKeyboard));

            // Check if the See Votes button already exists, if so keep the original keyboard.
            if (HasSeeVotesButton(inlineKeyboard))
                return new InlineKeyboardMarkup(inlineKeyboard.InlineKeyboard);

            // Add the See Votes button to the keyboard.
            return new InlineKeyboardMarkup(AddSeeVotesButton(track, inlineKeyboard));
        }

        // TODO: move to arguments.
        public static string GetVoteButtonText(VoteType voteType)
        {
            return voteType switch
            {
                VoteType.Upvote => UpvoteButtonText,
                VoteType.Downvote => DownvoteButtonText,
                _ => throw new NotImplementedException($"{nameof(VoteType)} {voteType} is not implemented in the {nameof(KeyboardService)}."),
            };
        }

        /// <summary>
        /// Check if two keyboards are configured the same way.
        /// </summary>
        public bool AreSame(InlineKeyboardMarkup firstKeyboard, InlineKeyboardMarkup secondKeyboard)
        {
            var firstKeyboardRows = firstKeyboard.InlineKeyboard.ToList();
            var secondKeyboardRows = secondKeyboard.InlineKeyboard.ToList();

            return firstKeyboardRows.All(firstKeyboardRow =>
            {
                var firstKeyboardButtons = firstKeyboardRow.ToList();
                var secondKeyboardButtons = secondKeyboardRows[firstKeyboardRows.IndexOf(firstKeyboardRow)].ToList();

                return firstKeyboardRow.All(firstKeyboardButton =>
                    AreSame(firstKeyboardButton, secondKeyboardButtons[firstKeyboardButtons.IndexOf(firstKeyboardButton)])
                );
            });
        }

        /// <summary>
        /// Check if two keyboard buttons are configured the same way.
        /// </summary>
        private static bool AreSame(InlineKeyboardButton firstKeyboardButton, InlineKeyboardButton secondKeyboardButton)
        {
            return
                firstKeyboardButton.Text == secondKeyboardButton.Text &&
                firstKeyboardButton.Url == secondKeyboardButton.Url &&
                firstKeyboardButton.CallbackData == secondKeyboardButton.CallbackData &&
                firstKeyboardButton.SwitchInlineQuery == secondKeyboardButton.SwitchInlineQuery;
        }

        /// <summary>
        /// Add the See Votes button to a keyboard, that shows the votes of a track when clicked.
        /// </summary>
        private static List<List<InlineKeyboardButton>> AddSeeVotesButton(Track track, InlineKeyboardMarkup originalKeyboard = null)
        {
            var query = $"{InlineQueryCommand.GetVoteUsers.ToDescriptionString()} {track.PlaylistId} {track.Id}";

            // Create a button on the keybaord that starts an inline query with the GetVoteUsers command.
            var seeVotesButton = InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(SeeVotesButtonText, query);

            var rows = new List<List<InlineKeyboardButton>>();
            if (originalKeyboard != null)
                // Copy the original keyboard.
                foreach (var row in originalKeyboard.InlineKeyboard)
                    rows.Add(row.ToList());
            else
                rows.Add(new List<InlineKeyboardButton>());

            // If there is a downvote button it should be on the last row.
            var lastRow = rows.Last();
            var downvoteButton = lastRow.SingleOrDefault(x => x.CallbackData == DownvoteButtonText);

            if (downvoteButton != null)
                // Add the See Votes button after the downvote button.
                lastRow.Insert(lastRow.IndexOf(downvoteButton) + 1, seeVotesButton);
            else
                lastRow.Add(seeVotesButton);

            return rows;
        }

        /// <summary>
        /// Check if the See Votes button is on the keyboard.
        /// </summary>
        private static bool HasSeeVotesButton(InlineKeyboardMarkup originalKeyboard)
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

        private static Uri GetWebAppUri()
        {
            var baseUri = new Uri(_azureOptions.FunctionAppUrl);

            return new Uri(baseUri, $"api/{nameof(WebApp).ToLower()}");
        }
    }
}