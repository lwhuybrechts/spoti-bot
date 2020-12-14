namespace Spoti_bot.Library.Options
{
    public class TestOptions
    {
        /// <summary>
        /// The id of the chat the integrationtests will send messages to.
        /// </summary>
        public int TestChatId { get; set; }
        public int TestUserId { get; set; }
        public string TestUserFirstName { get; set; }
        /// <summary>
        /// The id of the spotify playlist that tracks will be added and be removed from during integrationtests.
        /// </summary>
        public string TestPlaylistId { get; set; }
        /// <summary>
        /// The id of the spotify track that will be added and removed from the playlist during integrationtests.
        /// </summary>
        public string TestTrackId { get; set; }
    }
}
