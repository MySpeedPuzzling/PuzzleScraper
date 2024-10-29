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
            else
            {
                services.AddScoped<IPuzzleStore, JsonPuzzleStore>();
                services.AddScoped<ICompetitionStore, JsonCompetitionStore>();
                services.AddScoped<IPlayerStore, JsonPlayerStore>();
            }
            return services;
        }
    }
}
