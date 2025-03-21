using Arctic.Puzzlers.Stores.APIStores;
using Arctic.Puzzlers.Stores.Filestore;
using Arctic.Puzzlers.Stores.MemoryStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arctic.Puzzlers.Stores
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataStores(this IServiceCollection services, IConfiguration configuration)
        {
            
            var storeType = configuration.GetStoreType();
            if (storeType?.ToLower() == "memory")
            {
                services.AddSingleton<IPuzzleStore, MemoryPuzzleStore>();
                services.AddSingleton<ICompetitionStore, MemoryCompetitionStore>();
                services.AddSingleton<IPlayerStore, MemoryPlayerStore>();
            }
            else if(storeType?.ToLower() == "filestore")
            {
                services.AddScoped<IPuzzleStore, JsonPuzzleStore>();
                services.AddScoped<ICompetitionStore, JsonCompetitionStore>();
                services.AddScoped<IPlayerStore, JsonPlayerStore>();
            }
            else
            {
                services.AddScoped<IPuzzleStore, JsonPuzzleStore>();
                services.AddSingleton<IPuzzleStore, MemoryPuzzleStore>();
                services.AddSingleton<ICompetitionStore, MemoryCompetitionStore>();
                services.AddHttpClient<IPlayerStore, PlayerApiClient>(client =>
                {
                    client.BaseAddress = new Uri(configuration.GetApiUrl() + "api/player/");
                });
                services.AddHttpClient<IResultApiClient, ResultApiClient>(client =>
                {
                    client.BaseAddress = new Uri(configuration.GetApiUrl() + "api/result/");
                });
                services.AddHttpClient<ICompetitionApiClient, CompetitionApiClient>(client =>
                {
                    client.BaseAddress = new Uri(configuration.GetApiUrl() + "api/competition/");
                });
                services.AddHttpClient<ICompetitionGroupApiClient, CompetitionGroupApiClient>(client =>
                {
                    client.BaseAddress = new Uri(configuration.GetApiUrl() + "api/competitiongroup/");
                });
                services.AddHttpClient<ICompetitionRoundApiClient, CompetitionRoundApiClient>(client =>
                {
                    client.BaseAddress = new Uri(configuration.GetApiUrl() + "api/competitionround/");
                });
            }
            return services;
        }
    }
}
