using System.Net;
using System.Text;
using System.Text.Json;
using Arctic.Puzzlers.Objects.DTO;

namespace Arctic.Puzzlers.Stores.APIStores
{
    public interface ICompetitionRoundApiClient
    {
        Task<CompetitionRoundDTO?> GetByIdAsync(Guid id);
        Task<List<CompetitionRoundDTO>> GetByGroupIdAsync(Guid groupId);
        Task<CompetitionRoundDTO> CreateOrUpdateAsync(CompetitionRoundDTO round);
    }

    public class CompetitionRoundApiClient : ICompetitionRoundApiClient
    {
        private readonly HttpClient m_httpClient;
        private readonly JsonSerializerOptions m_jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public CompetitionRoundApiClient(HttpClient httpClient)
        {
            m_httpClient = httpClient;
        }

        public async Task<CompetitionRoundDTO?> GetByIdAsync(Guid id)
        {
            var response = await m_httpClient.GetAsync($"{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CompetitionRoundDTO>(content, m_jsonOptions);
        }

        public async Task<List<CompetitionRoundDTO>> GetByGroupIdAsync(Guid groupId)
        {
            var response = await m_httpClient.GetAsync($"competitiongroup/{groupId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var rounds = JsonSerializer.Deserialize<List<CompetitionRoundDTO>>(content, m_jsonOptions);

            return rounds ?? new List<CompetitionRoundDTO>();
        }


        public async Task<CompetitionRoundDTO> CreateOrUpdateAsync(CompetitionRoundDTO round)
        {
            var json = JsonSerializer.Serialize(round, m_jsonOptions);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await m_httpClient.PutAsync($"{round.Id}", jsonContent);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parsedround = JsonSerializer.Deserialize<CompetitionRoundDTO>(content, m_jsonOptions);
                round = parsedround ?? round;
            }
            return round;
        }
    }
}