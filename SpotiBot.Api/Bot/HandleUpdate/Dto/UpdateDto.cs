using SpotiBot.Api.Bot.Chats;
using SpotiBot.Api.Bot.Users;
using SpotiBot.Library.BusinessModels.Bot;
using SpotiBot.Library.BusinessModels.Spotify;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpotiBot.Api.Bot.HandleUpdate.Dto
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
        public ParsedChat ParsedChat { get; set; }
        public ParsedUser ParsedUser { get; set; }
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
        public User User { get; set; }
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

        public UpdateDto(
            string parsedUpdateId,
            UpdateType? parsedUpdateType,
            int? parsedBotMessageId,
            ParsedChat parsedChat,
            ParsedUser parsedUser,
            string parsedTrackId,
            string parsedTextMessage,
            string parsedTextMessageWithLinks,
            InlineKeyboardMarkup parsedInlineKeyboard,
            string parsedData,
            Chat chat,
            User user,
            AuthorizationToken authorizationToken,
            Playlist playlist,
            Track track)
        {
            ParsedUpdateId = parsedUpdateId;
            ParsedUpdateType = parsedUpdateType;
            ParsedBotMessageId = parsedBotMessageId;
            ParsedChat = parsedChat;
            ParsedUser = parsedUser;
            ParsedTrackId = parsedTrackId;
            ParsedTextMessage = parsedTextMessage;
            ParsedTextMessageWithLinks = parsedTextMessageWithLinks;
            ParsedInlineKeyboard = parsedInlineKeyboard;
            ParsedData = parsedData;
            Chat = chat;
            User = user;
            AuthorizationToken = authorizationToken;
            Playlist = playlist;
            Track = track;
        }
    }
}
