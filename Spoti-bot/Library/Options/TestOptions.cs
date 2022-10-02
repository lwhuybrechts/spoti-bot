namespace SpotiBot.Library.Options
{
    public class TestOptions
    {
        /// <summary>
        /// The id of the private chat the integrationtests will send messages to.
        /// </summary>
        public int PrivateTestChatId { get; set; }
        /// <summary>
        /// The id of the group chat the integrationtests will send messages to.
        /// </summary>
        public int GroupTestChatId { get; set; }
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
