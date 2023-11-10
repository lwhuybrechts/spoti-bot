using SpotiBot.Library.ApiModels;
using SpotiBot.View.ViewModels;

namespace SpotiBot.View.Mappers
{
    public interface IUserStatMapper
    {
        List<UserStat> Map(User[] users, Track[] tracks, Upvote[] upvotes);
    }
}