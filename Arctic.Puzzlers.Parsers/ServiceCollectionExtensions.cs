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
            services.AddSingleton<PuzzleParserFactory>();
            services.AddSingleton<IPuzzlePageParser, RavensBurgerParser>();
            services.AddSingleton<IPuzzlePageParser, SchmidtParser>();
            services.AddSingleton<CompetitionParserFactory>();
            services.AddSingleton<ICompetitionParser,AepuzzParser>();
            return services;
        }
    }
}
