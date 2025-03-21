using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.DTO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Arctic.Puzzlers.Stores.APIStores
{
    public interface ICompetitionGroupApiClient
    {
        Task<CompetitionGroupDTO?> GetByIdAsync(Guid id);
        Task<List<CompetitionGroupDTO>> GetByCompetitionIdAsync(Guid competitionId);
        Task<CompetitionGroupDTO> CreateOrUpdateAsync(CompetitionGroupDTO group);
    }

    public class CompetitionGroupApiClient : ICompetitionGroupApiClient
    {
        private readonly HttpClient m_httpClient;
        private readonly JsonSerializerOptions m_jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public CompetitionGroupApiClient(HttpClient httpClient)
        {
            m_httpClient = httpClient;
        }

        public async Task<CompetitionGroupDTO?> GetByIdAsync(Guid id)
        {
            var response = await m_httpClient.GetAsync($"{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CompetitionGroupDTO>(content, m_jsonOptions);
        }

        public async Task<List<CompetitionGroupDTO>> GetByCompetitionIdAsync(Guid competitionId)
        {
            // Assumes an endpoint like: GET api/CompetitionGroup/competition/{competitionId}
            var response = await m_httpClient.GetAsync($"competition/{competitionId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CompetitionGroupDTO>>(content, m_jsonOptions) ?? new List<CompetitionGroupDTO>();
        }

        public async Task<CompetitionGroupDTO> CreateOrUpdateAsync(CompetitionGroupDTO group)
        {
            var json = JsonSerializer.Serialize(group, m_jsonOptions);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            // Assumes PUT endpoint at api/CompetitionGroup/{id}
            var response = await m_httpClient.PutAsync($"{group.Id}", jsonContent);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parsedgroup = JsonSerializer.Deserialize<CompetitionGroupDTO>(content, m_jsonOptions);
                group = parsedgroup ?? group;
            }
            return group;
        }
    }
}