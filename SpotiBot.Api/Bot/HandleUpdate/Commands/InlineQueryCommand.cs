using System.ComponentModel;

namespace SpotiBot.Api.Bot.HandleUpdate.Commands
{
    /// <summary>
    /// The list with supported inline queries and the text that triggers them.
    /// </summary>
    public enum InlineQueryCommand
    {
        [Description("upvotes")]
        [RequiresQuery]
        GetVoteUsers,

        [Description("connect")]
        Connect
    }
}