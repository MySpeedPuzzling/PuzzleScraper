using Arctic.Puzzlers.CLI.InputParsing;
using Arctic.Puzzlers.Parsers.CompetitionParsers;
using Arctic.Puzzlers.Parsers.PuzzleParsers;
using Microsoft.Extensions.DependencyInjection;

namespace Arctic.Puzzlers.Parsers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddParser(this IServiceCollection services)
        {
            services.AddScoped<PuzzleParserFactory>();
            services.AddScoped<IPuzzlePageParser, RavensBurgerParser>();
            services.AddScoped<IPuzzlePageParser, SchmidtParser>();
            services.AddScoped<CompetitionParserFactory>();
            services.AddScoped<ICompetitionParser, AepuzzParser>();
            services.AddHttpClient<ICompetitionParser, SpeedPuzzlingParser>();
            return services;
        }
    }
}
