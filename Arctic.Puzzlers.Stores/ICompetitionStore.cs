using Arctic.Puzzlers.Objects.CompetitionObjects;

namespace Arctic.Puzzlers.Stores
{
    public interface ICompetitionStore
    {
        public Task<bool> Store(Competition competition);
        public Task<bool> NeedToParse(string url);
        public Task<List<Competition>> GetAll();

        public Task<List<Competition>> GetByName(string name);
        public Task<List<PlayerCompetitionResult>> GetPlayerCompetitionResultByName(string name);
    }
}
