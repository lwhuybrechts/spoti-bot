using SpotiApiModels;
using SpotiView.ViewModels;

namespace SpotiView.Mappers
{
    public interface IUserStatMapper
    {
        List<UserStat> Map(User[] users, Track[] tracks, Upvote[] upvotes);
    }
}