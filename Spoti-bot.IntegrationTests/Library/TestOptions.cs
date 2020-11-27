namespace Spoti_bot.IntegrationTests.Library
{
    /// <summary>
    /// Hardcoded settings that are used in integrationtests.
    /// </summary>
    public class TestOptions
    {
        /// <summary>
        /// The id of the chat the tests will send messages to.
        /// </summary>
        public int TestChatId { get; } = -436006832;
        public int TestUserId { get; } = 1;
        public string TestUserFirstName { get; } = "Spoti-bot-test-user";
    }
}
