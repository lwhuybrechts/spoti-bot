using Spoti_bot.Bot.Interfaces;
using System;
using System.Text.RegularExpressions;

namespace Spoti_bot.Bot
{
    public class UpvoteHelper : IUpvoteHelper
    {
        public const string ButtonText = "👍";

        private static readonly Regex _upvoteRegex = new Regex("(.+👍\\+)(\\d+)$");

        public bool EndsWithUpvote(string text)
        {
            return _upvoteRegex.Match(text).Success;
        }

        public string AddUpvote(string text)
        {
            return $"{text} 👍+1";
        }

        public string IncrementUpvote(string text)
        {
            var groups = _upvoteRegex.Match(text).Groups;

            var textWithoutUpvoteCount = groups[1].Value;
            var upvoteCount = Convert.ToInt32(groups[2].Value);

            return $"{textWithoutUpvoteCount}{upvoteCount + 1}";
        }
    }
}
