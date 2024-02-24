using Arctic.Puzzlers.Objects.PuzzleObjects;

namespace Arctic.Puzzlers.Parsers.PuzzleParsers
{
    public interface IPuzzlePageParser 
    {
        Task Parse(string url);
        bool SupportsBrand(BrandName brandName);
    }
}
