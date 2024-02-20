using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
namespace Arctic.Puzzlers.Stores
{
    public interface IFullDataStorage
    {
        public Task StoreAllPuzzleData(List<Competition> competitions, List<PuzzleExtended> puzzles);
        public bool SupportedStoreType(string storeType);
        
    }
}
