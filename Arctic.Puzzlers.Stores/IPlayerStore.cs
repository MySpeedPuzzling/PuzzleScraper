using Arctic.Puzzlers.Objects.DTO;


namespace Arctic.Puzzlers.Stores
{
    public interface IPlayerStore
    {
        public Task<bool> CreateOrUpdatePlayerAsync(Guid id, PlayerDTO player);

        public Task<PlayerDTO?> GetPlayerByIdAsync(Guid id);

        public Task<IEnumerable<PlayerDTO>?> GetPlayersByNameAsync(string name);
    }
}
