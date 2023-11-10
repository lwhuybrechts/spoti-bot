using SpotiBot.Library.ApiModels;

namespace SpotiBot.View.ViewModels
{
    public class UserStat
    {
        public User User { get; set; }
        public int SharedTrackAmount { get; set; }
        public int GaveUpvotesAmount { get; set; }
        public int ReceivedTrackUpvoteAmount { get; set; }
        public int ReceivedTrackUpvotePercentage { get; set; }
        public int ReceivedTotalUpvoteAmount { get; set; }
        public Track? MostUpvotedTrack { get; set; }
        public int MostUpvotedTrackAmount { get; set; }

        public UserStat(User user, int sharedTracksAmount, int gaveUpvotesAmount, int receivedTrackUpvoteAmount, int receivedTrackUpvotePercentage, int receivedTotalUpvoteAmount, Track? mostUpvotedTrack, int mostUpvotedTrackAmount)
        {
            User = user;
            SharedTrackAmount = sharedTracksAmount;
            GaveUpvotesAmount = gaveUpvotesAmount;
            ReceivedTrackUpvoteAmount = receivedTrackUpvoteAmount;
            ReceivedTrackUpvotePercentage = receivedTrackUpvotePercentage;
            ReceivedTotalUpvoteAmount = receivedTotalUpvoteAmount;
            MostUpvotedTrack = mostUpvotedTrack;
            MostUpvotedTrackAmount = mostUpvotedTrackAmount;
        }
    }
}
