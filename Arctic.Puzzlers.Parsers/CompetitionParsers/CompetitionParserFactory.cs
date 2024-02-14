using Microsoft.Extensions.Logging;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public class CompetitionParserFactory
    {
        private IEnumerable<ICompetitionParser> m_parsers;
        private ILogger<CompetitionParserFactory> m_logger;
        public CompetitionParserFactory(IEnumerable<ICompetitionParser> parsers, ILogger<CompetitionParserFactory> logger)
        {
            m_parsers = parsers;
            m_logger = logger;
        }

        public ICompetitionParser? GetParser(CompetitionType competitionType)
        {
            var returnvalue = m_parsers.FirstOrDefault(t => t.SupportCompetitionType(competitionType));
            m_logger.LogInformation($"Could not find any parser for {competitionType} ");
            return returnvalue;
        }
    }
}
