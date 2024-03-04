using Arctic.Puzzlers.Stores.Filestore;
using Arctic.Puzzlers.Stores.MemoryStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arctic.Puzzlers.Stores
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataStores(this IServiceCollection services, IConfiguration configuration)
        {
            
            var storeType = configuration.GetStoreType();
            if (!string.IsNullOrEmpty(storeType) || storeType.ToLower() == "memory")
            {
                services.AddSingleton<IPuzzleStore, MemoryPuzzleStore>();
                services.AddSingleton<ICompetitionStore, MemoryCompetitionStore>();
            }
            else
            {
                services.AddScoped<IPuzzleStore, JsonPuzzleStore>();
                services.AddScoped<ICompetitionStore, JsonCompetitionStore>();
            }
            return services;
        }
    }
}
