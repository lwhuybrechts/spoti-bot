using SpotiView.Mappers;

namespace SpotiView
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpotiViewClient(this IServiceCollection services)
        {
            return services
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
