using Arctic.Puzzlers.Objects.CompetitionObjects;

namespace Arctic.Puzzlers.Stores
{
    public interface ICompetitionStore : IDataStore
    {
        public Task<bool> Store(CompetitionRound competition);
        public Task<bool> NeedToParse(string url);
    }
}
