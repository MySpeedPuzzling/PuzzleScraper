using Arctic.Puzzlers.Parsers.PuzzleParsers;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public class WorldJigsawPuzzle : ISingleCompetitionParser
    {
        public Task ParseAndStore(string url, BrandName name, string shortid)
        {
            throw new NotImplementedException();
        }

        public Task ParseAndStore(string url)
        {
            throw new NotImplementedException();
        }

        public bool SupportCompetitionType(CompetitionType competitionType)
        {
            return competitionType.Equals(CompetitionType.WJPCSingle);
        }
    }
}
