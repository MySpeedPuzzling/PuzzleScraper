using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Logging;

namespace Arctic.Puzzlers.Parsers.PuzzleParsers
{
    public class PuzzleParserFactory
    {
        private IEnumerable<ICompetitionParser> m_parsers;
        public PuzzleParserFactory(IEnumerable<ICompetitionParser> parsers) 
        {
            m_parsers = parsers;
        }

        public ICompetitionParser GetParser(BrandName brandName)
        {
            var returnvalue = m_parsers.First(t => t.SupportsBrand(brandName));
            return returnvalue;
        }
    }
}
