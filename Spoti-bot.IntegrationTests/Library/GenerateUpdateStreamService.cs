using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Spoti_bot.Bot;
using Spoti_bot.Bot.Votes;
using Spoti_bot.Library.Options;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Spoti_bot.IntegrationTests
{
    /// <summary>
    /// Generates a stream with a serialized update. This can be used as a http request body, as input for tests.
    /// </summary>
    public class GenerateUpdateStreamService
    {
        private readonly TestOptions _testOptions;
        private readonly IKeyboardService _keyboardService;

        public GenerateUpdateStreamService(IOptions<TestOptions> testOptions, IKeyboardService keyboardService)
        {
            _testOptions = testOptions.Value;
            _keyboardService = keyboardService;
        }

        /// <summary>
        /// Write an update with a text message to the stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="text">The text to use in the text message.</param>
        public Task WriteTextMessageToStream(Stream stream, string text)
        {
            var update = new Telegram.Bot.Types.Update
            {
                Message = new Telegram.Bot.Types.Message()
                {
                    Text = text,
                    Date = DateTime.UtcNow,
                    From = GetTestUser(),
                    Chat = GetTestChat(),
                    Entities = new Telegram.Bot.Types.MessageEntity[0]
                }
            };

            return WriteUpdateToStream(stream, update);
        }

        /// <summary>
        /// Write a vote callback query to the stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="voteType">The type of the vote.</param>
        /// <param name="messageId">The id of the message that the callback was triggered by.</param>
        /// <param name="messageText">The text of the message that the callback was triggered by.</param>
        /// <param name="trackUrl">The url in de original message the bot replied to.</param>
        public Task WriteVoteCallbackQueryToStream(Stream stream, VoteType voteType, int messageId, string messageText, string trackUrl)
        {
            var update = new Telegram.Bot.Types.Update
            {
                CallbackQuery = new Telegram.Bot.Types.CallbackQuery
                {
                    Id = "testId",
                    Data = KeyboardService.GetVoteButtonText(voteType),
                    From = GetTestUser(),
                    Message = new Telegram.Bot.Types.Message()
                    {
                        Text = messageText,
                        Date = DateTime.UtcNow,
                        From = GetTestUser(isBot: true),
                        Chat = GetTestChat(),
                        Entities = new Telegram.Bot.Types.MessageEntity[0],
                        ReplyMarkup = _keyboardService.CreatePostedTrackResponseKeyboard(),
                        ReplyToMessage = new Telegram.Bot.Types.Message
                        {
                            From = GetTestUser(),
                            Chat = GetTestChat(),
                            Date = DateTime.UtcNow,
                            Text = trackUrl
                        },
                        MessageId = messageId,
                    },
                    ChatInstance = "testChatInstance"
                }
            };

            return WriteUpdateToStream(stream, update);
        }

        public Task WriteInlineQueryToStream(Stream stream, string query)
        {
            var update = new Telegram.Bot.Types.Update
            {
                InlineQuery = new Telegram.Bot.Types.InlineQuery
                {
                    Id = "testId",
                    From = GetTestUser(),
                    Query = query,
                    Offset = ""
                }
            };

            return WriteUpdateToStream(stream, update);
        }

        /// <summary>
        /// Serializes an update and writes it to the stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="update">The update to write.</param>
        private async Task WriteUpdateToStream(Stream stream, Telegram.Bot.Types.Update update)
        {
            var jsonString = JsonConvert.SerializeObject(update);

            await stream.WriteAsync(Encoding.UTF8.GetBytes(jsonString));

            // Make sure the stream can be read from by resetting it's position.
            stream.Position = 0;
        }

        private Telegram.Bot.Types.Chat GetTestChat()
        {
            return new Telegram.Bot.Types.Chat
            {
                Id = _testOptions.TestChatId,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
        }

        private Telegram.Bot.Types.User GetTestUser(bool isBot = false)
        {
            return new Telegram.Bot.Types.User
            {
                Id = _testOptions.TestUserId,
                FirstName = _testOptions.TestUserFirstName,
                IsBot = isBot
            };
        }
    }
}
