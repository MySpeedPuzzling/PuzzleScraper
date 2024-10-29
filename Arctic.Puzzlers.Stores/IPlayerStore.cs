using Arctic.Puzzlers.Objects.CompetitionObjects;


namespace Arctic.Puzzlers.Stores
{
    public interface IPlayerStore
    {
        public Task<bool> Store(Player player);

        public Task<Player?> GetPlayerByGuid(Guid id);

        public Task<IEnumerable<Player>> FindPlayerByName(string name);
    }
}
