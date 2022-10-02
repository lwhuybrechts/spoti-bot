using Microsoft.AspNetCore.Components;
using SpotiApiModels;
using System.Net.Http.Json;

namespace SpotiView.Pages
{
    public class IndexBase : ComponentBase
    {
        protected Track[] tracks = Array.Empty<Track>();

        [Inject]
        private HttpClient _httpClient { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var tracksTask = _httpClient.GetFromJsonAsync<Track[]>("/api/Tracks");

                tracks = HandleResponse(await tracksTask);
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
    }
}
