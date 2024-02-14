using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Logging;

namespace Arctic.Puzzlers.Parsers.PuzzleParsers
{
    public class PuzzleParserFactory
    {
        private IEnumerable<IPuzzlePageParser> m_parsers;
        private ILogger<PuzzleParserFactory> m_logger;
        public PuzzleParserFactory(IEnumerable<IPuzzlePageParser> parsers, ILogger<PuzzleParserFactory> logger) 
        {
            m_parsers = parsers;
            m_logger = logger;
        }

        public IPuzzlePageParser? GetParser(BrandName brandName)
        {
            var returnvalue = m_parsers.FirstOrDefault(t => t.SupportsBrand(brandName));
            m_logger.LogInformation($"Could not find any parser for {brandName} ");
            return returnvalue;
        }
    }
}
