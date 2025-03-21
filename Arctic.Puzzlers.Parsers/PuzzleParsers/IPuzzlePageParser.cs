using Arctic.Puzzlers.Objects.PuzzleObjects;

namespace Arctic.Puzzlers.Parsers.PuzzleParsers
{
    public interface ICompetitionParser 
    {
        Task Parse(string url);
        bool SupportsBrand(BrandName brandName);
    }
}
