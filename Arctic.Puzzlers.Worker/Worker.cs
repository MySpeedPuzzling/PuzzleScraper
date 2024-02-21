using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Arctic.Puzzlers.Parsers.CompetitionParsers;
using Arctic.Puzzlers.Parsers.PuzzleParsers;
using Arctic.Puzzlers.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;

namespace Arctic.Puzzlers.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory m_serviceScopeFactory;
        private readonly IConfiguration m_configuration;
        private readonly List<Tuple<string,BrandName>> m_brandUrls = new List<Tuple<string, BrandName>>()
        {
            new Tuple<string,BrandName>("https://www.schmidtspiele.de/puzzles-437.html?label=Schmidt+Spiele&thema=&kat=Erwachsenenpuzzle#filter", BrandName.Schmidt),
            new Tuple<string, BrandName>("https://www.ravensburger.co.uk/en-GB/products/jigsaw-puzzles", BrandName.Ravensburger),
            new Tuple<string, BrandName>("https://www.ravensburger.org/no/produkte/puslespill/index.html", BrandName.Ravensburger)
        };

        private readonly List<Tuple<string, CompetitionType>> m_competitionUrls = new List<Tuple<string, CompetitionType>>()
        {
            new Tuple<string, CompetitionType>("https://aepuzz.es/usuarios/clasificacion.php", CompetitionType.AePuzz)
        };

        private readonly List<Tuple<string, CompetitionType, BrandName, string>> m_singlecompetitionUrls = new List<Tuple<string, CompetitionType, BrandName,string>>()
        {
            new Tuple<string, CompetitionType,BrandName, string>("https://www.worldjigsawpuzzle.org/wjpc/2019/individual/final", CompetitionType.WJPCSingle, BrandName.Ravensburger,  "")
        };

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            m_serviceScopeFactory = serviceScopeFactory;
            m_configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                await DoWorkAsync(stoppingToken);
                var runMode = m_configuration.GetRunMode();
                if(!string.IsNullOrEmpty(runMode) && runMode.ToLower().Equals("once"))
                {
                    break;
                }
            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            using (IServiceScope scope = m_serviceScopeFactory.CreateScope())
            {
                var puzzleList = new List<PuzzleExtended>();
                var competitionList = new List<Competition>();
                try
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    }
                    var competitionFactory = scope.ServiceProvider.GetRequiredService<CompetitionParserFactory>();
                    foreach (var competitionUrl in m_competitionUrls)
                    {
                        var parser = competitionFactory.GetParser(competitionUrl.Item2);

                        if (parser != null)
                        {
                            competitionList.AddRange(await parser.Parse(competitionUrl.Item1));
                        }

                    }
                    var puzzleFactory = scope.ServiceProvider.GetRequiredService<PuzzleParserFactory>();
                    foreach (var brandPage in m_brandUrls)
                    {
                        var parser = puzzleFactory.GetParser(brandPage.Item2);

                        if (parser != null)
                        {
                            puzzleList.AddRange(await parser.Parse(brandPage.Item1));
                        }

                    }
                    await scope.ServiceProvider.GetRequiredService<FullDataStoreFactory>().StoreData(competitionList, puzzleList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "something went wrong");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {

            await base.StopAsync(stoppingToken);
        }
    }
}
