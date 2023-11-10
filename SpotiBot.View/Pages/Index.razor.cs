using Microsoft.AspNetCore.Components;
using SpotiBot.Library.ApiModels;
using SpotiBot.Library.Spotify.Api;
using System.Net.Http.Json;
using Track = SpotiBot.Library.BusinessModels.Spotify.Track;

namespace SpotiBot.View.Pages
{
    public class IndexBase : ComponentBase, IDisposable
    {
        protected List<Track> _tracks = new();

        [Inject] private HttpClient _httpClient { get; set; } = default!;
        [Inject] private StateContainer _stateContainer { get; set; } = default!;
        [Inject] private IMapper _mapper { get; set; } = default!;
        [Inject] private ISpotifyClientFactory _spotifyClientFactory { get; set; } = default!;
        [Inject] private ISpotifyClientService _spotifyClientService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _stateContainer.OnChange += StateHasChanged;

                if (_stateContainer.SpotifyAccessToken == null)
                    return;
                
                var client = _spotifyClientFactory.Create(_stateContainer.SpotifyAccessToken);
                
                // For now, only return tracks from the main playlist.
                const string playlistId = "2tnyzyB8Ku9XywzAYNjLxj";

                var tracksTask = _httpClient.GetFromJsonAsync<Library.ApiModels.Track[]>("/api/Tracks");
                var apiTracksTask = _spotifyClientService.GetTracks(client, playlistId);

                var tracks = await tracksTask;
                var apiTracks = await apiTracksTask;
                
                if (tracks == null || !tracks.Any())
                    return;

                var orderedTracks = tracks.OrderByDescending(x => x.AddedAt).ToList();

                _tracks = _mapper.Map(orderedTracks, apiTracks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Dispose()
        {
            _stateContainer.OnChange -= StateHasChanged;
        }
    }
}
