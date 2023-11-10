using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using SpotiBot.Data.Options;
using SpotiBot.Data.Repositories;
using SpotiBot.Data.Services;
using SpotiBot.Library;

namespace SpotiBot.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddData(this IServiceCollection services)
        {
            return services
                .AddLibrary()
                .AddMapper()
                .AddServices()
                .AddRepositories()
                .AddStorage()
                .AddOptions();
        }

        private static IServiceCollection AddMapper(this IServiceCollection services)
        {
            return services
                .AddTransient<IMapper, Mapper>();
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IAuthorizationTokenService, AuthorizationTokenService>()
                .AddTransient<IChatService, ChatService>()
                .AddTransient<ILoginRequestService, LoginRequestService>()
                .AddTransient<IPlaylistService, PlaylistService>()
                .AddTransient<ITrackService, TrackService>()
                .AddTransient<IUserService, UserService>()
                .AddTransient<IVoteService, VoteService>();
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IAuthorizationTokenRepository, AuthorizationTokenRepository>()
                .AddTransient<IChatMemberRepository, ChatMemberRepository>()
                .AddTransient<IChatRepository, ChatRepository>()
                .AddTransient<ILoginRequestRepository, LoginRequestRepository>()
                .AddTransient<IPlaylistRepository, PlaylistRepository>()
                .AddTransient<ITrackRepository, TrackRepository>()
                .AddTransient<IUserRepository, UserRepository>()
                .AddTransient<IVoteRepository, VoteRepository>();
        }

        /// <summary>
        /// Add classes that are needed for data storage.
        /// </summary>
        private static IServiceCollection AddStorage(this IServiceCollection services)
        {
            // Add the CloudTableClient as a singleton.
            return services.AddSingleton((serviceProvider) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AzureOptions>>().Value;

                var storageAccount = CloudStorageAccount.Parse(options.StorageAccountConnectionString);
                var cloudTableClient = storageAccount.CreateCloudTableClient();

                return cloudTableClient;
            });
        }

        private static IServiceCollection AddOptions(this IServiceCollection services)
        {
            return services
                .AddAndBindOptions<AzureOptions>();
        }
    }
}
