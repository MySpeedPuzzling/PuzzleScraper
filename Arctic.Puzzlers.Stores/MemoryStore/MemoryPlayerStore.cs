using Arctic.Puzzlers.Objects.CompetitionObjects;
using Microsoft.Extensions.Configuration;

namespace Arctic.Puzzlers.Stores.MemoryStore
{
    public class MemoryPlayerStore : IPlayerStore
    {
        private List<Player> m_players;
        public MemoryPlayerStore()
        {
            m_players = new List<Player>();
        }
        public Task<IEnumerable<Player>> FindPlayerByName(string name)
        {
            var players = m_players.Where(p => p.FullName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return Task.FromResult(players);
        }

        public Task<Player?> GetPlayerByGuid(Guid id)
        {
            var player = m_players.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(player);
        }

        public Task<bool> Store(Player player)
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
