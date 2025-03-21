using Arctic.Puzzlers.Objects.DTO;

namespace Arctic.Puzzlers.Stores.MemoryStore
{
    public class MemoryPlayerStore : IPlayerStore
    {
        private List<PlayerDTO> m_players;
        public MemoryPlayerStore()
        {
            m_players = new List<PlayerDTO>();
        }
        public Task<IEnumerable<PlayerDTO>> GetPlayersByNameAsync(string name)
        {
            var players = m_players.Where(p => p.FullName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return Task.FromResult(players);
        }

        public Task<PlayerDTO?> GetPlayerByIdAsync(Guid id)
        {
            var player = m_players.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(player);
        }

        public Task<bool> CreateOrUpdatePlayerAsync(Guid id, PlayerDTO player)
        {
            if (!m_players.Any(t => t.FullName == player.FullName))
            {
                m_players.Add(player);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
