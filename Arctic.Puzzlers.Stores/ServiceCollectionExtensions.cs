using Arctic.Puzzlers.Stores.Filestore;
using Microsoft.Extensions.DependencyInjection;

namespace Arctic.Puzzlers.Stores
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataStores(this IServiceCollection services)
        {
            services.AddScoped<FullDataStoreFactory>();
            services.AddScoped<IFullDataStorage, JsonFileStore>();
            return services;

        }
    }
}
