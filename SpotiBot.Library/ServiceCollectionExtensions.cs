using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotiBot.Library.ApiModels;
using SpotiBot.Library.Options;
using SpotiBot.Library.Spotify.Api;

namespace SpotiBot.Library
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLibrary(this IServiceCollection services)
        {
            return services
                .AddApiModels()
                .AddOptions()
                .AddSpotifyApi();
        }

        public static IServiceCollection AddAndBindOptions<T>(this IServiceCollection services) where T : class
        {
            var optionsName = typeof(T).Name;

            // Add the values from the configuration to this options' registration.
            services
                .AddOptions<T>()
                .Configure<IConfiguration>((options, configuration) =>
                    configuration.GetSection(optionsName).Bind(options))
                .ValidateDataAnnotations();

            return services;
        }

        private static IServiceCollection AddApiModels(this IServiceCollection services)
        {
            return services
                .AddTransient<IMapper, Mapper>()
                .AddTransient<Spotify.Authorization.IMapper, Spotify.Authorization.Mapper>()
                .AddTransient<Spotify.Tracks.IMapper, Spotify.Tracks.Mapper>()
                .AddTransient<Spotify.Playlists.IMapper, Spotify.Playlists.Mapper>();
        }

        private static IServiceCollection AddOptions(this IServiceCollection services)
        {
            return services
                .AddAndBindOptions<SpotifyOptions>();
        }

        private static IServiceCollection AddSpotifyApi(this IServiceCollection services)
        {
            return services
                .AddTransient<ISpotifyClientFactory, SpotifyClientFactory>()
                .AddTransient<ISpotifyClientService, SpotifyClientService>();
        }
    }
}
