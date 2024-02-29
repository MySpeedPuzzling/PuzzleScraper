
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Configuration;

namespace Arctic.Puzzlers.Stores.MemoryStore
{
    public class MemoryPuzzleStore : IPuzzleStore
    {
        private readonly IConfiguration m_configuration;
        private List<PuzzleExtended> m_puzzleList;
        public MemoryPuzzleStore(IConfiguration config)
        {
            m_puzzleList = new List<PuzzleExtended>();
            m_configuration = config;
        }       

        public Task<bool> Store(PuzzleExtended puzzle)
        {
            if (!m_puzzleList.Any(t => puzzle.BrandName == t.BrandName && puzzle.ShortId == t.ShortId))
            {
                m_puzzleList.Add(puzzle);
                return Task.FromResult(true);
            }
            if (m_configuration.OverrideData())
            {
                m_puzzleList.RemoveAll(t => puzzle.BrandName == t.BrandName && puzzle.ShortId == t.ShortId);
                m_puzzleList.Add(puzzle);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> NeedToParse(BrandName brandName, long shortId)
        {
            return Task.FromResult(!m_puzzleList.Any(t => brandName == t.BrandName && shortId == t.ShortId) || m_configuration.OverrideData());
        }

        public Task<bool> NeedToParse(string url)
        {
            return Task.FromResult(!m_puzzleList.Any(t => url == t.Url) || m_configuration.OverrideData());
        }
        public Task<List<PuzzleExtended>> GetAll()
        {
            return Task.FromResult(m_puzzleList);
        }

        public Task<PuzzleExtended?> GetByBrandNameAndId(BrandName brandName, long shortid)
        {
            return Task.FromResult(m_puzzleList.FirstOrDefault(t => brandName == t.BrandName && shortid == t.ShortId));
        }

      
    }
}
