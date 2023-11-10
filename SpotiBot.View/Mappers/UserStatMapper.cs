using SpotiBot.Library.ApiModels;
using SpotiBot.View.ViewModels;

namespace SpotiBot.View.Mappers
{
    public class UserStatMapper : IUserStatMapper
    {
        public List<UserStat> Map(User[] users, Track[] tracks, Upvote[] upvotes)
        {
            var userStats = new List<UserStat>();

            foreach (User user in users)
                userStats.Add(Map(user, tracks, upvotes));

            return userStats;
        }

        private static UserStat Map(User user, Track[] tracks, Upvote[] upvotes)
        {
            var userTracks = tracks
                .Where(x => x.AddedByTelegramUserId.ToString() == user.Id)
                .ToList();

            var userTrackIds = userTracks
                .Select(x => x.Id)
                .ToList();

            var userTrackUpvotes = upvotes
                .Where(x => userTrackIds.Contains(x.TrackId))
                .ToList();

            var gameUpvotesAmount = upvotes
                .Count(x => x.UserId.ToString() == user.Id);

            var userTrackMostUpvotesAmount = userTrackUpvotes
                .GroupBy(x => x.TrackId)
                .Select(x => x.Count())
                .DefaultIfEmpty(0)
                .Max();

            var userTrackWithMostUpvotes = GetUserTrackWithMostUpvotes(userTrackMostUpvotesAmount, userTrackUpvotes, userTracks);

            var userTrackIdsWithUpvote = userTrackUpvotes
                .DistinctBy(x => x.TrackId)
                .Count();

            var userTracksWithUpvotePercentage = userTrackIds.Count > 0
                ? Convert.ToInt32((double)userTrackIdsWithUpvote / userTrackIds.Count * 100)
                : 0;

            return new UserStat(
                user,
                userTrackIds.Count,
                gameUpvotesAmount,
                userTrackIdsWithUpvote,
                userTracksWithUpvotePercentage,
                userTrackUpvotes.Count,
                userTrackWithMostUpvotes,
                userTrackMostUpvotesAmount
            );
        }

        private static Track? GetUserTrackWithMostUpvotes(int userTrackMostUpvotesAmount, List<Upvote> userTrackUpvotes, List<Track> userTracks)
        {
            if (userTrackMostUpvotesAmount <= 0)
                return null;

            var userTrackIdWithMostUpvotes = userTrackUpvotes
                .GroupBy(x => x.TrackId)
                .Where(x => x.Count() == userTrackMostUpvotesAmount)
                .FirstOrDefault()
                ?.FirstOrDefault()
                ?.TrackId ?? string.Empty;

            return userTracks
                .FirstOrDefault(x => x.Id == userTrackIdWithMostUpvotes);
        }
    }
}
