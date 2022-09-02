using System.Collections.Generic;

namespace Spoti_bot.Bot.Votes
{
    public interface IVoteTextHelper
    {
        string IncrementVote(string text, VoteType voteType);
        string DecrementVote(string text, VoteType voteType);
        string ReplaceVotes(string text, List<Vote> votes);
    }
}
