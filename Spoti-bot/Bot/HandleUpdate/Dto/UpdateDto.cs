using Spoti_bot.Bot.Chats;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Playlists;

namespace Spoti_bot.Bot.HandleUpdate.Dto
{
    /// <summary>
    /// Holds objects related to the Update that was sent.
    /// </summary>
    public class UpdateDto
    {
        /// <summary>
        /// The parsed Telegram update object.
        /// </summary>
        public Telegram.Bot.Types.Update Update { get; set; }

        public Users.User ParsedUser { get; set; }
        public Chat ParsedChat { get; set; }
        public string ParsedTextMessage => Update?.Message?.Text
                ?? Update?.CallbackQuery?.Message?.Text;

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
    }
}
