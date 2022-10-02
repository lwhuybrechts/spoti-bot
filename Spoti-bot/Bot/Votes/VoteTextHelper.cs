using SpotiBot.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpotiBot.Bot.Votes
{
    /// <summary>
    /// Takes care of displaying the votes and their count at the end of a text message.
    /// </summary>
    public class VoteTextHelper : IVoteTextHelper
    {
        /// <summary>
        /// Increment a vote in the text message, or add it when it's missing.
        /// </summary>
        /// <param name="text">The text to increment the vote for.</param>
        /// <param name="voteType">The VoteType to increment.</param>
        /// <returns>The text with the incremented vote.</returns>
        public string IncrementVote(string text, VoteType voteType)
        {
            return HandleVote(text, voteType, true);
        }

        /// <summary>
        /// Decrement a vote in the text message, or add it when it's missing.
        /// </summary>
        /// <param name="text">The text to decrement the vote for.</param>
        /// <param name="voteType">The VoteType to decrement.</param>
        /// <returns>The text with the decremented vote.</returns>
        public string DecrementVote(string text, VoteType voteType)
        {
            return HandleVote(text, voteType, false);
        }

        /// <summary>
        /// Replace the votes in a text.
        /// </summary>
        /// <returns>A text with the updated votes.</returns>
        public string ReplaceVotes(string text, List<Vote> votes)
        {
            var newText = GetTextWithoutVotes(text);

            foreach (var vote in votes)
                newText = UseNegativeOperator(vote.Type)
                    ? DecrementVote(newText, vote.Type)
                    : IncrementVote(newText, vote.Type);

            return newText;
        }

        /// <summary>
        /// Strip the votes from a text.
        /// </summary>
        private static string GetTextWithoutVotes(string text)
        {
            var voteTypes = Enum.GetValues(typeof(VoteType)).Cast<VoteType>().ToList();
            var match = GetRegex(voteTypes).Match(text);

            return match.Success
                ? match.Groups[1].Value
                : text;
        }

        /// <summary>
        /// Handle the vote.
        /// </summary>
        private static string HandleVote(string text, VoteType voteType, bool shouldIncrement)
        {
            var voteTypes = Enum.GetValues(typeof(VoteType)).Cast<VoteType>().ToList();
            var match = GetRegex(voteTypes).Match(text);
            var groups = match.Groups;

            if (match.Success)
            {
                var textWithoutVotes = groups[1].Value;

                return $"{textWithoutVotes}{GetVoteTexts(groups, voteTypes, voteType, shouldIncrement)}";
            }

            return $"{text}{GetVoteTexts(groups, voteTypes, voteType, shouldIncrement)}";
        }


        /// <summary>
        /// Get the text with all Votes and their counts.
        /// </summary>
        /// <param name="groups">The groups that the regex match resulted in.</param>
        /// <param name="voteTypes">All voteTypes.</param>
        /// <param name="currentVoteType">The voteType to increment or decrement.</param>
        /// <param name="shouldIncrement">Increment the voteType if true, decrement it if false.</param>
        private static string GetVoteTexts(GroupCollection groups, List<VoteType> voteTypes, VoteType currentVoteType, bool shouldIncrement)
        {
            var voteTexts = string.Empty;

            // Add all existing votes and increment or decrement the current vote.
            foreach (var voteType in voteTypes)
                voteTexts += GetVoteText(groups, voteTypes, voteType, voteType == currentVoteType ? shouldIncrement : (bool?)null);

            return voteTexts;
        }

        /// <summary>
        /// Get the text for a Vote, that contains a text and a count.
        /// </summary>
        private static string GetVoteText(GroupCollection groups, List<VoteType> voteTypes, VoteType voteType, bool? shouldIncrement)
        {
            var voteText = groups[GetVoteTextIndex(voteTypes, voteType)].Value;
            int.TryParse(groups[GetVoteCountIndex(voteTypes, voteType)].Value, out var voteCount);

            if (shouldIncrement.HasValue)
            {
                if (string.IsNullOrEmpty(voteText))
                    return AddNewVoteText(voteType, shouldIncrement);

                if (shouldIncrement.Value)
                {
                    if (!UseNegativeOperator(voteType))
                        voteCount++;
                    else
                        voteCount--;
                }
                else
                {
                    if (!UseNegativeOperator(voteType))
                        voteCount--;
                    else
                        voteCount++;
                }
            }

            // Don't add the vote if the count is 0.
            if (voteCount == 0)
                return string.Empty;

            return $"{voteText}{voteCount}";
        }

        /// <summary>
        /// Get the index of the regex groups for the text of this VoteType.
        /// </summary>
        /// <param name="voteTypes">All VoteTypes.</param>
        /// <param name="voteType">The VoteType to get the index for.</param>
        /// <returns>The index in the regex groups that contains the match for the text of this VoteType.</returns>
        private static int GetVoteTextIndex(List<VoteType> voteTypes, VoteType voteType)
        {
            // Votes start matching from the 2nd index.
            const int voteTypeIndexStart = 2;

            // Votes have two matches each, the text and the count.
            return voteTypeIndexStart + voteTypes.IndexOf(voteType) * 2;
        }

        /// <summary>
        /// Get the index of the regex groups for the count of this VoteType.
        /// </summary>
        /// <param name="voteTypes">All VoteTypes.</param>
        /// <param name="voteType">The VoteType to get the index for.</param>
        /// <returns>The index in the regex groups that contains the match for the count of this VoteType</returns>
        private static int GetVoteCountIndex(List<VoteType> voteTypes, VoteType voteType)
        {
            // The vote count index is after it's text.
            return GetVoteTextIndex(voteTypes, voteType) + 1;
        }

        /// <summary>
        /// Add the VoteType if it was missing.
        /// </summary>
        /// <returns>The text for the VoteType.</returns>
        private static string AddNewVoteText(VoteType voteType, bool? shouldIncrement)
        {
            var useNegativeOperator = UseNegativeOperator(voteType);

            if (shouldIncrement.Value && useNegativeOperator)
                throw new NotSupportedException("A negative voteType cannot be incremented when it isn't added yet.");
            else if (!shouldIncrement.Value && !useNegativeOperator)
                throw new NotSupportedException("A positive voteTypecannot be decremented when it isn't added yet.");

            var voteCountOperator = useNegativeOperator ? "-" : "+";
            const int voteCount = 1;

            // Add a space, then the voteTypeText and operator, then the count.
            return $" {KeyboardService.GetVoteButtonText(voteType)}{voteCountOperator}{voteCount}";
        }

        /// <summary>
        /// Create a Regex to match with at least one VoteType.
        /// </summary>
        private static Regex GetRegex(List<VoteType> voteTypes)
        {
            // Match the textstring and at least one vote.
            var pattern = "(.+?)(?!$)";

            foreach (var voteType in voteTypes)
            {
                // Some voteTypes use a negative operator.
                var voteCountOperator = UseNegativeOperator(voteType)
                    ? "-"
                    : "\\+";

                var voteTypeText = KeyboardService.GetVoteButtonText(voteType);

                // Match a space, then the voteTypeText and operator, then the count.
                pattern += $"(?:(\\s{voteTypeText}{voteCountOperator})(\\d+))?";
            }

            pattern += "$";

            return new Regex(pattern, RegexOptions.Singleline);
        }

        private static bool UseNegativeOperator(VoteType voteType)
        {
            return voteType.HasAttribute<VoteType, VoteAttributes.UseNegativeOperatorAttribute>();
        }
    }
}