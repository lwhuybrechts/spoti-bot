using SpotiBot.View.Mappers;

namespace SpotiBot.View
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpotiView(this IServiceCollection services)
        {
            return services
                .AddSingleton<IStateContainer, StateContainer>()
                .AddTransient<IUserStatMapper, UserStatMapper>()
                .AddScoped(provider =>
                {
                    var configuration = provider.GetRequiredService<IConfiguration>();

                    return new HttpClient
                    {
                        BaseAddress = new Uri(configuration["API_Prefix"])
                    };
                });
        }
    }
}
