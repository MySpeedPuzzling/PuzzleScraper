using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.DTO;
using Npgsql;
using System.Data.Common;
using System.Text.Json;

namespace Arctic.Puzzlers.Webapi.DB
{
    public interface ICompetitionDBStore
    {
        public Task<CompetitionDTO?> GetByIdAsync(Guid id);
        public Task<List<CompetitionDTO>> GetAllAsync();
        public Task<CompetitionDTO?> GetByUniqueNameAsync(string name);
        public Task<bool> InsertAsync(CompetitionDTO competition);
        public Task<bool> UpdateAsync(CompetitionDTO competition);    
    }
    public class CompetitionDBStore : ICompetitionDBStore
    {
        private readonly NpgsqlDataSource m_datasource;
        public CompetitionDBStore(NpgsqlDataSource datasource)
        {
            m_datasource = datasource;
        }

        public async Task<List<CompetitionDTO>> GetAllAsync()
        {
            using (var cmd = m_datasource.CreateCommand("SELECT data FROM jigsaw.competition "))
            {
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var competitions = new List<CompetitionDTO>();
                    while (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        var competition = JsonSerializer.Deserialize<CompetitionDTO>(jsonData);
                        if (competition != null)
                        {
                            competitions.Add(competition);
                        }
                    }
                    return competitions;
                }
            }
        }

        public async Task<CompetitionDTO?> GetByIdAsync(Guid id)
        {
            await using (var cmd = m_datasource.CreateCommand("SELECT data FROM jigsaw.competition WHERE id = @id"))
            {
                cmd.Parameters.AddWithValue("id", id);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to ComptitionDTO object
                        var resultDto = JsonSerializer.Deserialize<CompetitionDTO>(jsonData);
                        return resultDto;
                    }
                }
            }
            return null;
        }

        public async Task<CompetitionDTO?> GetByUniqueNameAsync(string name)
        {
            await using (var cmd = m_datasource.CreateCommand("SELECT data FROM jigsaw.competition WHERE uniquename = @uniquename"))
            {
                cmd.Parameters.AddWithValue("uniquename", name);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var jsonData = reader.GetString(0);
                        // Deserialize jsonData to ComptitionDTO object
                        var resultDto = JsonSerializer.Deserialize<CompetitionDTO>(jsonData);
                        return resultDto;
                    }
                }
            }
            return null;
        }

        public async Task<bool> InsertAsync(CompetitionDTO competition)
        {
            await using (var cmd = m_datasource.CreateCommand("INSERT INTO jigsaw.competition (id,name, uniquename,startdate, data) VALUES (@id, @name, @uniquename, @startdate , @data::jsonb)"))
            {
                cmd.Parameters.AddWithValue("id", competition.Id);
                cmd.Parameters.AddWithValue("name", competition.Name);
                cmd.Parameters.AddWithValue("uniquename", competition.UniqueName);
                cmd.Parameters.AddWithValue("startdate", competition.Time);
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(competition));

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public async Task<bool> UpdateAsync(CompetitionDTO competition)
        {
            await using (var cmd = m_datasource.CreateCommand("UPDATE jigsaw.competition SET name = @name, uniquename = @uniquename, startdate = @startdate, data = @data::jsonb WHERE id = @id"))
            {
                cmd.Parameters.AddWithValue("id", competition.Id);
                cmd.Parameters.AddWithValue("name", competition.Name);
                cmd.Parameters.AddWithValue("uniquename", competition.UniqueName);
                cmd.Parameters.AddWithValue("startdate", competition.Time);
                cmd.Parameters.AddWithValue("data", JsonSerializer.Serialize(competition));

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
        }
    }
}