using Spoti_bot.Bot.Chats;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Playlists;
using Spoti_bot.Spotify.Tracks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot.HandleUpdate.Dto
{
    /// <summary>
    /// Holds objects related to the Update that was sent.
    /// </summary>
    public class UpdateDto
    {
        #region ParsedObjects
        public string ParsedUpdateId { get; set; }
        public UpdateType? ParsedUpdateType { get; set; }
        public int? ParsedBotMessageId { get; set; }
        public Chat ParsedChat { get; set; }
        public Users.User ParsedUser { get; set; }
        public string ParsedTrackId { get; set; }
        public string ParsedTextMessage { get; set; }
        public string ParsedTextMessageWithLinks { get; set; }
        public InlineKeyboardMarkup ParsedInlineKeyboard { get; set; }
        public string ParsedData { get; set; }
        #endregion

        #region ObjectFromStorage
        /// <summary>
        /// The chat that the update was sent in.
        /// </summary>
        public Chat Chat { get; set; }
        /// <summary>
        /// The user that sent the update.
        /// </summary>
        public Users.User User { get; set; }
        /// <summary>
        /// The Spotify authorizationToken of the user.
        /// </summary>
        public AuthorizationToken AuthorizationToken { get; set; }
        /// <summary>
        /// The playlist that was set for the chat.
        /// </summary>
        public Playlist Playlist { get; set; }
        /// <summary>
        /// The saved track that the message contains.
        /// </summary>
        public Track Track { get; set; }
        #endregion
    }
}
