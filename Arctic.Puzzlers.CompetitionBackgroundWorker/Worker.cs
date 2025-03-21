using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Arctic.Puzzlers.Parsers.CompetitionParsers;

namespace Arctic.Puzzlers.CompetitionBackgroundWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory m_serviceScopeFactory;
        private readonly IConfiguration m_configuration;
        private Timer m_timer;
        private static bool m_timerrunning;

        private readonly List<Tuple<string, CompetitionOwner>> m_competitionUrls = new List<Tuple<string, CompetitionOwner>>()
        {
            new Tuple<string, CompetitionOwner>("https://speedpuzzle.eu/Results", CompetitionOwner.SpeedPuzzleEu),
            new Tuple<string, CompetitionOwner>("https://www.speedpuzzling.com/results.html", CompetitionOwner.SpeedPuzzling),
            new Tuple<string, CompetitionOwner>("https://www.speedpuzzling.com/results2024.html", CompetitionOwner.SpeedPuzzling),
            new Tuple<string, CompetitionOwner>("https://www.speedpuzzling.com/results-2023.html", CompetitionOwner.SpeedPuzzling),
            new Tuple<string, CompetitionOwner>("https://www.speedpuzzling.com/results-2022.html", CompetitionOwner.SpeedPuzzling),
            new Tuple<string, CompetitionOwner>("https://www.speedpuzzling.com/results-2021.html", CompetitionOwner.SpeedPuzzling),
            new Tuple<string, CompetitionOwner>("https://www.speedpuzzling.com/results-2020.html", CompetitionOwner.SpeedPuzzling),
            new Tuple<string, CompetitionOwner>("https://www.worldjigsawpuzzle.org/", CompetitionOwner.WJPC),
            new Tuple<string, CompetitionOwner>("https://aepuzz.es/usuarios", CompetitionOwner.AePuzz)
        };

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            m_serviceScopeFactory = serviceScopeFactory;
            m_configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var runMode = m_configuration.GetRunMode();
            if (!string.IsNullOrEmpty(runMode) && runMode.ToLower().Equals("once"))
            {

                await DoWorkAsync(stoppingToken);
                return;
            }
            if (!string.IsNullOrEmpty(runMode) && TimeSpan.TryParse(runMode, out TimeSpan waitTime))
            {
                m_timer = new Timer(async o => await DoWorkAsync(stoppingToken), null, TimeSpan.Zero, waitTime);

            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            if (m_timerrunning)
            {
                return;
            }
            try
            {
                m_timerrunning = true;
                using (IServiceScope scope = m_serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                        }
                        var typesToRun = m_configuration.GetParseTypes();
                        if (typesToRun.Any(t => t.ToLower() == "competition"))
                        {
                            var competitionFactory = scope.ServiceProvider.GetRequiredService<CompetitionParserFactory>();
                            foreach (var competitionUrl in m_competitionUrls)
                            {
                                try
                                {
                                    var parser = competitionFactory.GetParser(competitionUrl.Item2);
                                    if (parser != null)
                                    {
                                        await parser.Parse(competitionUrl.Item1);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Something whent wrong when parsing {competitionUrl}");
                                }
                            }
                        }                      
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "something went wrong");
                    }
                }
            }
            finally
            {
                m_timerrunning = false;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            m_timer?.Change(Timeout.Infinite, 0);
            await base.StopAsync(stoppingToken);
        }
    }
}
