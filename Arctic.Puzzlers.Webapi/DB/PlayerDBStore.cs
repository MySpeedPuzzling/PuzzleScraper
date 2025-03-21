using Arctic.Puzzlers.Objects.DTO;
using Npgsql;
using System.Text.Json;

namespace Arctic.Puzzlers.Webapi.DB
{
    public interface IPlayerDBStore
    {
        Task<bool> InsertAsync(PlayerDTO player);

        Task<bool> UpdateAsync(PlayerDTO player);
        Task<PlayerDTO?> GetByIdAsync(Guid id);

        Task<IEnumerable<PlayerDTO>> SearchByNameAsync(string name);
    }
    public class PlayerDBStore : IPlayerDBStore
    {
        private readonly NpgsqlDataSource m_datasource;
        public PlayerDBStore(NpgsqlDataSource datasource)
        {
            m_datasource = datasource;
        }
        public async Task<PlayerDTO?> GetByIdAsync(Guid id)
        {
            await using (var cmd = m_datasource.CreateCommand("SELECT data FROM jigsaw.player WHERE id = @id"))
            {

                cmd.Parameters.AddWithValue("id", id);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to Player object
                        var player = JsonSerializer.Deserialize<PlayerDTO>(jsonData);
                        return player;
                    }
                }
            }
            return null;
        }
        public async Task<bool> UpdateAsync(PlayerDTO player)
        {
            await using (var cmd = m_datasource.CreateCommand("UPDATE jigsaw.player SET name = @name, data = @data::jsonb WHERE id = @id"))
            {
                cmd.Parameters.AddWithValue("id", player.Id);
                cmd.Parameters.AddWithValue("name", player.FullName);
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(player));

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public async Task<bool> InsertAsync(PlayerDTO player)
        {
            await using (var cmd = m_datasource.CreateCommand("INSERT INTO jigsaw.player (id, name, data) VALUES (@id, @name, @data::jsonb)"))
            {
                cmd.Parameters.AddWithValue("id", player.Id);
                cmd.Parameters.AddWithValue("name", player.FullName);
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(player));

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public async Task<IEnumerable<PlayerDTO>> SearchByNameAsync(string name)
        {
            var players = new List<PlayerDTO>();
            await using (var cmd = m_datasource.CreateCommand("SELECT data FROM jigsaw.player WHERE LOWER(name) ILIKE '%' || @name || '%'"))
            {
                cmd.Parameters.AddWithValue("name", name.ToLower());
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to Player object
                        var player = JsonSerializer.Deserialize<PlayerDTO>(jsonData);
                        if (player != null)
                        {
                            players.Add(player);
                        }
                    }
                }
            }

            return players;
        }
    }
}
