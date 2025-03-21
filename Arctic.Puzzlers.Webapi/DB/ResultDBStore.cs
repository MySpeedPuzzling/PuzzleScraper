using Arctic.Puzzlers.Objects.DTO;
using Npgsql;
using System.Text.Json;

namespace Arctic.Puzzlers.Webapi.DB
{
    public interface IResultDBStore
    {
        Task<bool> InsertAsync(ResultDTO result);

        Task<bool> UpdateAsync(ResultDTO result);

        Task<List<ResultDTO>> GetByRoundIdAsync(Guid id);
        Task<ResultDTO?> GetByIdAsync(Guid id);
        Task<List<ResultDTO>> GetByPlayerIdAsync(Guid id);
        Task<ResultDTO?> GetByPlayerIdAndRoundIdAsync(Guid playerId, Guid competitionRoundId);
    }
    public class ResultDBStore : IResultDBStore
    {
        private readonly NpgsqlDataSource m_datasource;
        public ResultDBStore(NpgsqlDataSource datasource)
        {
            m_datasource = datasource;
        }
        public async Task<ResultDTO?> GetByIdAsync(Guid id)
        {
            await using (var cmd = m_datasource.CreateCommand("SELECT data FROM jigsaw.result WHERE id = @id"))
            {

                cmd.Parameters.AddWithValue("id", id);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to Player object
                        var resultDto = JsonSerializer.Deserialize<ResultDTO>(jsonData);
                        return resultDto;
                    }
                }
            }
            return null;
        }

        public async Task<ResultDTO?> GetByPlayerIdAndRoundIdAsync(Guid playerId, Guid competitionRoundId)
        {
            var results = new List<ResultDTO>();
            await using (var cmd = m_datasource.CreateCommand("SELECT r.data FROM jigsaw.result r join jigsaw.playerresult pr on pr.resultid = r.id WHERE pr.playerid = @id and r.competitionroundid = @roundId "))
            {
                cmd.Parameters.AddWithValue("id", playerId);
                cmd.Parameters.AddWithValue("roundId", competitionRoundId);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to Player object
                        var resultDto = JsonSerializer.Deserialize<ResultDTO>(jsonData);
                        return resultDto;
                    }
                }
            }
            return null;
        }

        public async Task<List<ResultDTO>> GetByPlayerIdAsync(Guid playerId)
        {
            var results = new List<ResultDTO>();
            await using (var cmd = m_datasource.CreateCommand("SELECT r.data FROM jigsaw.result r join jigsaw.playerresult pr on pr.resultid = r.id WHERE pr.playerid = @id"))
            {
                cmd.Parameters.AddWithValue("id", playerId);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to Player object
                        var resultDto = JsonSerializer.Deserialize<ResultDTO>(jsonData);
                        if(resultDto != null)
                        {
                            results.Add(resultDto);
                        }
                    }
                }
            }
            return results;
        }

        public async Task<List<ResultDTO>> GetByRoundIdAsync(Guid roundId)
        {
            var results = new List<ResultDTO>();
            await using (var cmd = m_datasource.CreateCommand("SELECT data FROM jigsaw.result WHERE competitionroundid = @roundId"))
            {
                cmd.Parameters.AddWithValue("roundId", roundId);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to Player object
                        var resultDto = JsonSerializer.Deserialize<ResultDTO>(jsonData);
                        if (resultDto != null)
                        {
                            results.Add(resultDto);
                        }
                    }
                }
            }
            return results;

        }

        public async Task<bool> InsertAsync(ResultDTO result)
        {
            await using var connection = await m_datasource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            await using (var cmd = new NpgsqlCommand("INSERT INTO jigsaw.result (id, competitionroundid, data) VALUES (@id, @competitionroundid, @data::jsonb)", connection, transaction))
            {
                cmd.Parameters.AddWithValue("id", result.Id);
                cmd.Parameters.AddWithValue("competitionroundid", result.CompetitionRoundId);
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(result));

                var response = await cmd.ExecuteNonQueryAsync();

                foreach (var player in result.PlayerIds)
                {
                    await InsertPlayerResult(player, result.Id, connection, transaction);
                }
                await transaction.CommitAsync();
                return response > 0;
            }
        }


        public async Task<bool> UpdateAsync(ResultDTO result)
        {
            await using var connection = await m_datasource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            await using (var cmd = new NpgsqlCommand("UPDATE jigsaw.result SET competitionroundid = @competitionroundid, data = @data::jsonb WHERE id = @id", connection, transaction))
            {
                cmd.Parameters.AddWithValue("id", result.Id);
                cmd.Parameters.AddWithValue("competitionroundid", result.CompetitionRoundId);   
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(result));

                var response = await cmd.ExecuteNonQueryAsync();
                foreach(var player in result.PlayerIds)
                {
                    await InsertPlayerResult(player, result.Id, connection, transaction);
                }
                await transaction.CommitAsync();
                return response > 0;
            }
        }

        private async Task<bool> InsertPlayerResult(Guid playerId, Guid ResultId, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            await using (var cmd = new NpgsqlCommand("SELECT * FROM jigsaw.playerresult WHERE playerid = @playerid and resultid = @resultid", connection, transaction))
            {
                cmd.Parameters.AddWithValue("playerid", playerId);
                cmd.Parameters.AddWithValue("resultid", ResultId);
                var response = await cmd.ExecuteNonQueryAsync();
                if(response > 0)
                {
                    return true;
                }
            }
            await using (var cmd = new NpgsqlCommand("INSERT INTO jigsaw.playerresult (id,playerid, resultid) VALUES (@id, @playerid, @resultid)", connection, transaction))
            {
                cmd.Parameters.AddWithValue("id", Guid.NewGuid());
                cmd.Parameters.AddWithValue("playerid", playerId);
                cmd.Parameters.AddWithValue("resultid", ResultId);
                var response = await cmd.ExecuteNonQueryAsync();
                return response > 0;
            }
        }
    }
}
