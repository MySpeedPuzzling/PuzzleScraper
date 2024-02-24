using Arctic.Puzzlers.Objects.PuzzleObjects;

namespace Arctic.Puzzlers.Stores
{
    public interface IPuzzleStore : IDataStore
    {
        public Task<bool> Store(PuzzleExtended puzzle);
        public Task<bool> NeedToParse(BrandName brandName, long ShortId);
        public Task<bool> NeedToParse(string url);
    }
}
