using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Arctic.Puzzlers.Parsers.CompetitionParsers;
using Arctic.Puzzlers.Parsers.PuzzleParsers;
using System.Security.Cryptography.X509Certificates;

namespace Arctic.Puzzlers.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly PuzzleParserFactory m_puzzleFactory;
        private readonly CompetitionParserFactory m_competitionFactory;
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

        public Worker(ILogger<Worker> logger, PuzzleParserFactory puzzleFactory, CompetitionParserFactory competitionFactory)
        {
            _logger = logger;
            m_puzzleFactory = puzzleFactory;
            m_competitionFactory = competitionFactory;                          
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var puzzleList = new List<PuzzleExtended>();
                var competitionList = new List<Competition>();
                try
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    }

                    foreach (var competitionUrl in m_competitionUrls)
                    {
                        var parser = m_competitionFactory.GetParser(competitionUrl.Item2);

                        if (parser != null)
                        {
                            competitionList.AddRange(await parser.Parse(competitionUrl.Item1));
                        }

                    }

                    foreach (var brandPage in m_brandUrls)
                    {
                        var parser = m_puzzleFactory.GetParser(brandPage.Item2);

                        if (parser != null)
                        {
                            puzzleList.AddRange(await parser.Parse(brandPage.Item1));
                        }

                    }
                                         
                    await Task.Delay(1 * 24 * 60 * 60 * 1000, stoppingToken);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "something went wrong");
                }
            }
        }
    }
}
