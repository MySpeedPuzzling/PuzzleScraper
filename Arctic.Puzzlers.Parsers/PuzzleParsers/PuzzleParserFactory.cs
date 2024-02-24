using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Logging;

namespace Arctic.Puzzlers.Parsers.PuzzleParsers
{
    public class PuzzleParserFactory
    {
        private IEnumerable<IPuzzlePageParser> m_parsers;
        public PuzzleParserFactory(IEnumerable<IPuzzlePageParser> parsers) 
        {
            m_parsers = parsers;
        }

        public IPuzzlePageParser GetParser(BrandName brandName)
        {
            var returnvalue = m_parsers.First(t => t.SupportsBrand(brandName));
            return returnvalue;
        }
    }
}
