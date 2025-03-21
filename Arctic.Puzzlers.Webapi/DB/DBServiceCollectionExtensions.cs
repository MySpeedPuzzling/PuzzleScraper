using Arctic.Puzzlers.Webapi.Extensions;

namespace Arctic.Puzzlers.Webapi.DB
{
    public static class DBServiceCollectionExtensions
    {
        public static IServiceCollection AddDbStores(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddNpgsqlDataSource(configuration.GetPostgresConnectionString());
            services.AddScoped<IPlayerDBStore, PlayerDBStore>();
            services.AddScoped<IResultDBStore, ResultDBStore>();
            services.AddScoped<ICompetitionRoundDBStore, CompetitionRoundDBStore>();
            services.AddScoped<ICompetitionGroupDBStore, CompetitionGroupDBStore>();
            services.AddScoped<ICompetitionDBStore, CompetitionDBStore>();
            return services;
        }
    }
}
