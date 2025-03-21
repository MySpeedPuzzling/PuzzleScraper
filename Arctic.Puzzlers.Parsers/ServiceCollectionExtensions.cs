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
            services.AddScoped<PuzzleParsers.ICompetitionParser, RavensBurgerParser>();
            services.AddScoped<PuzzleParsers.ICompetitionParser, SchmidtParser>();
            services.AddScoped<CompetitionParserFactory>();
            services.AddScoped<CompetitionParsers.ICompetitionParser, AepuzzParser>();
            services.AddScoped<CompetitionParsers.ICompetitionParser, WorldJigsawPuzzleOrgParser>();
            services.AddScoped<CompetitionParsers.ICompetitionParser, SpeedPuzzlingParser>();
            services.AddScoped<CompetitionParsers.ICompetitionParser, SpeedPuzzleEuParser>();
            return services;
        }
    }
}
