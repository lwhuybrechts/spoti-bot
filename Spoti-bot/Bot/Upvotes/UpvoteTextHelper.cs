using System;
using System.Text.RegularExpressions;

namespace Spoti_bot.Bot.Upvotes
{
    public class UpvoteTextHelper : IUpvoteTextHelper
    {
        public const string ButtonText = "👍";

        private static readonly Regex _upvoteRegex = new Regex("(.+)(\\s👍\\+)(\\d+)$");

        public string IncrementUpvote(string text)
        {
            if (!EndsWithUpvote(text))
                return AddUpvote(text);

            var groups = _upvoteRegex.Match(text).Groups;

            var textWithoutUpvote = groups[1].Value;
            var upvote = groups[2].Value;
            var upvoteCount = Convert.ToInt32(groups[3].Value);

            return $"{textWithoutUpvote}{upvote}{upvoteCount + 1}";
        }

        public string DecrementUpvote(string text)
        {
            if (!EndsWithUpvote(text))
                return text;

            var groups = _upvoteRegex.Match(text).Groups;

            var textWithoutUpvote = groups[1].Value;
            var upvote = groups[2].Value;
            var upvoteCount = Convert.ToInt32(groups[3].Value);

            if (upvoteCount - 1 <= 0)
                return $"{textWithoutUpvote}";

            return $"{textWithoutUpvote}{upvote}{upvoteCount - 1}";
        }

        private bool EndsWithUpvote(string text)
        {
            return _upvoteRegex.Match(text).Success;
        }

        private string AddUpvote(string text)
        {
            return $"{text} 👍+1";
        }
    }
}
