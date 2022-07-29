namespace Spoti_bot.Library
{
    public enum BotResponseCode
    {
        NoAction,
        TestCommandHandled,
        StartCommandHandled,
        HelpCommandHandled,
        SetPlaylistCommandHandled,
        GetLoginLinkCommandHandled,
        ResetCommandHandled,
        CommandRequirementNotFulfilled,
        TrackAlreadyExists,
        TrackAddedToPlaylist,
        TrackRemovedFromPlaylist,
        AddVoteHandled,
        RemoveVoteHandled,
        AddToQueueHandled,
        InlineQueryHandled,
        ExceptionHandled,
        WebAppHandled
    }
}
