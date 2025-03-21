using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.DTO;
using Npgsql;
using System.Data.Common;
using System.Text.Json;

namespace Arctic.Puzzlers.Webapi.DB
{
    public interface ICompetitionGroupDBStore
    {
        public Task<CompetitionGroupDTO?> GetByIdAsync(Guid id);
        public Task<List<CompetitionGroupDTO>> GetByCompetitionIdAsync(Guid competitionid);
        public Task<bool> Insert(CompetitionGroupDTO competitionGroup);
        public Task<bool> Update(CompetitionGroupDTO competitionGroup);
        public Task<CompetitionGroupDTO?> GetByCompetitionIdAndType(Guid competitionId, ContestType contestType);
    }
    public class CompetitionGroupDBStore : ICompetitionGroupDBStore
    {
        private readonly NpgsqlDataSource m_dataSource;
        public CompetitionGroupDBStore(NpgsqlDataSource dataSource) 
        {
            m_dataSource = dataSource;
        }

        public async Task<CompetitionGroupDTO?> GetByCompetitionIdAndType(Guid competitionId, ContestType contestType)
        {
            using(var cmd = m_dataSource.CreateCommand("SELECT data FROM jigsaw.competitiongroup WHERE competitionid = @competitionid and contesttype = @contesttype"))
            {
                cmd.Parameters.AddWithValue("competitionid", competitionId);
                cmd.Parameters.AddWithValue("contestType", contestType.ToString());
                await using (var reader = await cmd.ExecuteReaderAsync())
                {                    
                    if (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to ComptitionDTO object
                        var resultDto = JsonSerializer.Deserialize<CompetitionGroupDTO>(jsonData);
                        return resultDto;
                    }
                }
            }
            return null;
        }

        public async Task<List<CompetitionGroupDTO>> GetByCompetitionIdAsync(Guid competitionid)
        {
            using (var cmd = m_dataSource.CreateCommand("SELECT data FROM jigsaw.competitiongroup WHERE competitionid = @competitionid"))
            {
                cmd.Parameters.AddWithValue("competitionid", competitionid);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var groups = new List<CompetitionGroupDTO>();
                    while (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        var group = JsonSerializer.Deserialize<CompetitionGroupDTO>(jsonData);
                        if (group != null)
                        {
                            groups.Add(group);
                        }
                    }
                    return groups;
                }
            }

        }

        public async Task<CompetitionGroupDTO?> GetByIdAsync(Guid id)
        {
            await using (var cmd = m_dataSource.CreateCommand("SELECT data FROM jigsaw.competitiongroup WHERE id = @id"))
            {
                cmd.Parameters.AddWithValue("id", id);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to ComptitionDTO object
                        var resultDto = JsonSerializer.Deserialize<CompetitionGroupDTO>(jsonData);
                        return resultDto;
                    }
                }
            }
            return null;
        }

        public async Task<bool> Insert(CompetitionGroupDTO competitionGroup)
        {
            await using (var cmd = m_dataSource.CreateCommand("INSERT INTO jigsaw.competitiongroup (id,competitionid,contesttype, data) VALUES (@id, @competitionid ,@contesttype, @data::jsonb)"))
            {
                cmd.Parameters.AddWithValue("id", competitionGroup.Id);
                cmd.Parameters.AddWithValue("competitionid", competitionGroup.CompetitionId);
                cmd.Parameters.AddWithValue("contesttype", competitionGroup.ContestType.ToString());
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(competitionGroup));

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public async Task<bool> Update(CompetitionGroupDTO competitionGroup)
        {
            await using (var cmd = m_dataSource.CreateCommand("UPDATE jigsaw.competitiongroup SET competitionid = @competitionid, contesttype=@contesttype, data = @data::jsonb WHERE id = @id"))
            {
                cmd.Parameters.AddWithValue("id", competitionGroup.Id);
                cmd.Parameters.AddWithValue("competitionid", competitionGroup.CompetitionId);
                cmd.Parameters.AddWithValue("contesttype", competitionGroup.ContestType.ToString());
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(competitionGroup));

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
        }
    }
}