using Microsoft.Extensions.Logging;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public class CompetitionParserFactory
    {
        private IEnumerable<ICompetitionParser> m_parsers;
        public CompetitionParserFactory(IEnumerable<ICompetitionParser> parsers)
        {
            m_parsers = parsers;
        }

        public ICompetitionParser? GetParser(CompetitionType competitionType)
        {
            var returnvalue = m_parsers.First(t => t.SupportCompetitionType(competitionType));
            return returnvalue;
        }
    }
}
