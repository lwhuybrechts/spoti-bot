using SpotiBot.Library.Enums;
using SpotiBot.Library.BusinessModels.Bot;
using System.Collections.Generic;

namespace SpotiBot.Api.Bot.Votes
{
    public interface IVoteTextHelper
    {
        string IncrementVote(string text, VoteType voteType);
        string DecrementVote(string text, VoteType voteType);
        string ReplaceVotes(string text, List<Vote> votes);
    }
}
