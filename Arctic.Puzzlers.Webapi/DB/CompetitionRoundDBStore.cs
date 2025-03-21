using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.DTO;
using Npgsql;
using System.Data.Common;
using System.Numerics;
using System.Text.Json;

namespace Arctic.Puzzlers.Webapi.DB
{
    public interface ICompetitionRoundDBStore
    {
        public Task<CompetitionRoundDTO?> GetByIdAsync(Guid id);
        public Task<List<CompetitionRoundDTO>> GetByGroupId(Guid competitionId);
        
        public Task<bool> Insert(CompetitionRoundDTO competitionRound);

        public Task<bool> Update(CompetitionRoundDTO competitionRound);
        public Task<CompetitionRoundDTO?> GetByNameAndGroupId(string roundName, Guid competitionGroupId);
    }
    public class CompetitionRoundDBStore: ICompetitionRoundDBStore
    {
        private readonly NpgsqlDataSource m_dataSource;
        public CompetitionRoundDBStore(NpgsqlDataSource dataSource)
        {
            m_dataSource = dataSource;
        }

        public async Task<List<CompetitionRoundDTO>> GetByGroupId(Guid competitiongroupid)
        {
            using (var cmd = m_dataSource.CreateCommand("SELECT data FROM jigsaw.competitionround WHERE competitiongroupid = @competitiongroupid"))
            {
                cmd.Parameters.AddWithValue("competitiongroupid", competitiongroupid);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var rounds = new List<CompetitionRoundDTO>();
                    while (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        var round = JsonSerializer.Deserialize<CompetitionRoundDTO>(jsonData);
                        if (round != null)
                        {
                            rounds.Add(round);
                        }
                    }
                    return rounds;
                }
            }
        }

        public async Task<CompetitionRoundDTO?> GetByIdAsync(Guid id)
        {
            using (var cmd = m_dataSource.CreateCommand("SELECT data FROM jigsaw.competitionround WHERE id = @id"))
            {
                cmd.Parameters.AddWithValue("id", id);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        var round = JsonSerializer.Deserialize<CompetitionRoundDTO>(jsonData);
                        return round;
                    }               
                }
            }
            return null;
        }

        public async Task<CompetitionRoundDTO?> GetByNameAndGroupId(string roundName, Guid competitionGroupId)
        {
            using (var cmd = m_dataSource.CreateCommand("SELECT data FROM jigsaw.competitionround WHERE competitiongroupid = @competitiongroupid and name= @name"))
            {
                cmd.Parameters.AddWithValue("competitiongroupid", competitionGroupId);
                cmd.Parameters.AddWithValue("name", roundName);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        var round = JsonSerializer.Deserialize<CompetitionRoundDTO>(jsonData);
                        return round;
                    }
                }
            }
            return null;
        }

        public async Task<bool> Insert(CompetitionRoundDTO competitionRound)
        {
            await using (var cmd = m_dataSource.CreateCommand("INSERT INTO jigsaw.competitionround (id,competitiongroupid, name,startdate, data) VALUES (@id, @competitiongroupid, @name, @startdate , @data::jsonb)"))
            {
                cmd.Parameters.AddWithValue("id", competitionRound.Id);
                cmd.Parameters.AddWithValue("competitiongroupid", competitionRound.CompetitionGroupId);
                cmd.Parameters.AddWithValue("name", competitionRound.RoundName);
                cmd.Parameters.AddWithValue("startdate", competitionRound.Time);
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(competitionRound));

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public async Task<bool> Update(CompetitionRoundDTO competitionRound)
        {
            await using (var cmd = m_dataSource.CreateCommand("UPDATE jigsaw.competitionround SET name = @name, competitiongroupid = @competitiongroupid, startdate = @startdate, data = @data::jsonb WHERE id = @id"))
            {
                cmd.Parameters.AddWithValue("id", competitionRound.Id);
                cmd.Parameters.AddWithValue("competitiongroupid", competitionRound.CompetitionGroupId);
                cmd.Parameters.AddWithValue("name", competitionRound.RoundName);
                cmd.Parameters.AddWithValue("startdate", competitionRound.Time);
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(competitionRound));

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
        }
    }
}
