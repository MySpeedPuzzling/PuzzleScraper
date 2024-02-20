using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Logging;

namespace Arctic.Puzzlers.Stores
{
    public class FullDataStoreFactory
    {
        private IEnumerable<IFullDataStorage> m_stores;
        private ILogger<FullDataStoreFactory> m_logger;
        public FullDataStoreFactory(IEnumerable<IFullDataStorage> parsers, ILogger<FullDataStoreFactory> logger)
        {
            m_stores = parsers;
            m_logger = logger;
        }

        public async Task StoreData( List<Competition> competitions, List<PuzzleExtended> puzzles)
        {
            foreach(IFullDataStorage store in m_stores)
            {
                await store.StoreAllPuzzleData(competitions, puzzles);
            }
        }
    }
}
