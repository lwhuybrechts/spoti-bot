using Microsoft.AspNetCore.Components;
using SpotiBot.Library.ApiModels;
using SpotiBot.View.Mappers;
using SpotiBot.View.ViewModels;
using System.Net.Http.Json;

namespace SpotiBot.View.Pages
{
    public class StatsBase : ComponentBase
    {
        protected List<UserStat> userStats = new();

        [Inject]
        private IUserStatMapper _userStatMapper { get; set; } = default!;
        [Inject]
        private HttpClient _httpClient { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var usersTask = _httpClient.GetFromJsonAsync<User[]>("/api/Users");
                var tracksTask = _httpClient.GetFromJsonAsync<Track[]>("/api/Tracks");
                var upvotesTask = _httpClient.GetFromJsonAsync<Upvote[]>("/api/Upvotes");

                var users = HandleResponse(await usersTask);
                var tracks = HandleResponse(await tracksTask);
                var upvotes = HandleResponse(await upvotesTask);

                userStats = _userStatMapper.Map(users, tracks, upvotes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Track[] HandleResponse(Track[]? tracks)
        {
            if (tracks == null)
                return Array.Empty<Track>();

            return tracks;
        }

        private static User[] HandleResponse(User[]? users)
        {
            if (users == null)
                return Array.Empty<User>();

            return users
                .OrderBy(x => x.FirstName)
                .ToArray();
        }

        private static Upvote[] HandleResponse(Upvote[]? upvotes)
        {
            if (upvotes == null)
                return Array.Empty<Upvote>();

            return upvotes;
        }
    }
}
