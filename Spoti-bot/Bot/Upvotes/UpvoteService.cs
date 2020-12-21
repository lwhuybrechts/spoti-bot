using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify;
using Spoti_bot.Spotify.Tracks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot.Upvotes
{
    public class UpvoteService : IUpvoteService
    {
        private readonly ISendMessageService _sendMessageService;
        private readonly IKeyboardService _keyboardService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly IUpvoteTextHelper _upvoteTextHelper;
        private readonly IUpvoteRepository _upvoteRepository;
        private readonly ITrackRepository _trackRepository;

        public UpvoteService(
            ISendMessageService sendMessageService,
            IKeyboardService keyboardService,
            ISpotifyLinkHelper spotifyLinkHelper,
            IUpvoteTextHelper upvoteTextHelper,
            IUpvoteRepository upvoteRepository,
            ITrackRepository trackRepository)
        {
            _sendMessageService = sendMessageService;
            _keyboardService = keyboardService;
            _spotifyLinkHelper = spotifyLinkHelper;
            _upvoteTextHelper = upvoteTextHelper;
            _upvoteRepository = upvoteRepository;
            _trackRepository = trackRepository;
        }

        /// <summary>
        /// Check if a callback query is an upvote callback.
        /// </summary>
        /// <param name="callbackQuery">The callback query to check.</param>
        /// <returns>True if the callback query is an upvote callback.</returns>
        public bool IsUpvoteCallback(CallbackQuery callbackQuery)
        {
            return callbackQuery.Data.Equals(KeyboardService.UpvoteButtonText);
        }

        /// <summary>
        /// Check if a message contains an upvote callback and if so, handle it.
        /// </summary>
        /// <param name="callbackQuery">The callback query to handle.</param>
        public async Task<BotResponseCode> TryHandleUpvote(UpdateDto updateDto)
        {
            if (!IsUpvoteCallback(updateDto.Update.CallbackQuery))
                return BotResponseCode.NoAction;

            var track = await GetTrack(updateDto);

            // Every callback query should have a track.
            if (track == null)
                throw new TrackIdNullException();

            return await HandleUpvote(updateDto, track);
        }

        /// <summary>
        /// Get the track from the callback query.
        /// </summary>
        private async Task<Track> GetTrack(UpdateDto updateDto)
        {
            // Get the message with the trackId.
            var trackMessageText = updateDto.Update.CallbackQuery.Message.ReplyToMessage.Text;

            var trackId = await _spotifyLinkHelper.ParseTrackId(trackMessageText);

            if (string.IsNullOrEmpty(trackId))
                return null;

            return await _trackRepository.Get(trackId, updateDto.Playlist.Id);
        }

        /// <summary>
        /// Upvote a track, or if it was already upvoted remove the upvote.
        /// </summary>
        private async Task<BotResponseCode> HandleUpvote(UpdateDto updateDto, Track track)
        {
            var newUpvote = new Upvote
            {
                PlaylistId = track.PlaylistId,
                TrackId = track.Id,
                UserId = updateDto.ParsedUser.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var text = GetTextMessageWithTextLinks(updateDto.Update.CallbackQuery.Message);

            var existingUpvote = await _upvoteRepository.Get(newUpvote);

            return existingUpvote == null
                ? await Upvote(updateDto, newUpvote, text, track)
                : await Downvote(updateDto, existingUpvote, text, track);
        }

        private async Task<BotResponseCode> Upvote(UpdateDto updateDto, Upvote upvote, string text, Track track)
        {
            // Save the upvote in storage.
            await _upvoteRepository.Upsert(upvote);

            var message = updateDto.Update.CallbackQuery.Message;
            var keyboard = await _keyboardService.GetUpdatedUpvoteKeyboard(message, track);

            // Increment upvote in the original message.
            var newText = _upvoteTextHelper.IncrementUpvote(text);
            await EditOriginalMessage(updateDto, message, newText, keyboard);
            
            return BotResponseCode.UpvoteHandled;
        }

        private async Task<BotResponseCode> Downvote(UpdateDto updateDto, Upvote existingUpvote, string text, Track track)
        {
            // Delete the upvote from storage.
            await _upvoteRepository.Delete(existingUpvote);

            var message = updateDto.Update.CallbackQuery.Message;
            var keyboard = await _keyboardService.GetUpdatedUpvoteKeyboard(message, track);

            // Decrement upvote in the original message.
            var newText = _upvoteTextHelper.DecrementUpvote(text);
            await EditOriginalMessage(updateDto, message, newText, keyboard);

            return BotResponseCode.DownvoteHandled;
        }

        // TODO: move to a central place?
        /// <summary>
        /// Get the original text and re-add the links to it.
        /// We need to do this since the telegram library we use doesn't offer a way to pass the entities.
        /// </summary>
        /// <param name="textMessage">The message we want the text from.</param>
        /// <returns>The text with all links added to it.</returns>
        private string GetTextMessageWithTextLinks(Message textMessage)
        {
            var text = textMessage.Text;

            foreach (var textLinkEntity in textMessage.Entities.Where(x => x.Type == MessageEntityType.TextLink))
            {
                var firstPart = text.Substring(0, textLinkEntity.Offset);
                var linkText = text.Substring(firstPart.Length, textLinkEntity.Length);
                var lastPart = text.Substring(firstPart.Length + linkText.Length);

                text = $"{firstPart}[{linkText}]({textLinkEntity.Url}){lastPart}";
            }

            return text;
        }

        /// <summary>
        /// Edit the message that triggered the callback query.
        /// </summary>
        /// <param name="message">The message to edit.</param>
        /// <param name="newText">The new text to replace the message text with.</param>
        private async Task EditOriginalMessage(UpdateDto updateDto, Message message, string newText, InlineKeyboardMarkup replyMarkup)
        {
            await _sendMessageService.EditMessageText(
                updateDto.Chat.Id,
                message.MessageId,
                newText,
                replyMarkup: replyMarkup);
        }
    }
}