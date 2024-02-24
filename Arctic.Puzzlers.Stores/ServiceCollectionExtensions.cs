using Arctic.Puzzlers.Stores.Filestore;
using Microsoft.Extensions.DependencyInjection;

namespace Arctic.Puzzlers.Stores
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataStores(this IServiceCollection services)
        {
            services.AddScoped<IPuzzleStore, JsonPuzzleStore>();
            services.AddScoped<ICompetitionStore, JsonCompetitionStore>();
            return services;
        }
    }
}
