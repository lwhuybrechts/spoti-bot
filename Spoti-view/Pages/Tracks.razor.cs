using Microsoft.AspNetCore.Components;
using SpotiApiModels;
using System.Net.Http.Json;

namespace SpotiView.Pages
{
    public class TracksBase : ComponentBase
    {
        protected Track[] tracks = Array.Empty<Track>();
        protected Dictionary<string, User> users = new();

        [Inject]
        private HttpClient _httpClient { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var tracksTask = _httpClient.GetFromJsonAsync<Track[]>("/api/Tracks");
                var usersTask = _httpClient.GetFromJsonAsync<User[]>("/api/Users");

                tracks = HandleResponse(await tracksTask);
                users = HandleResponse(await usersTask);
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

            return tracks
                .OrderByDescending(x => x.AddedAt)
                .ToArray();
        }

        private static Dictionary<string, User> HandleResponse(User[]? users)
        {
            if (users == null)
                return new Dictionary<string, User>();

            return users.ToDictionary(x => x.Id);
        }
    }
}
