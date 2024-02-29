using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;

namespace Arctic.Puzzlers.Stores
{
    public interface ICompetitionStore
    {
        public Task<bool> Store(Competition competition);
        public Task<bool> NeedToParse(string url);
        public Task<List<Competition>> GetAll();
    }
}
