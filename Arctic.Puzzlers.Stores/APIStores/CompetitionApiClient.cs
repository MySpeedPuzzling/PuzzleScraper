using System.Net;
using System.Text;
using System.Text.Json;
using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.DTO;

namespace Arctic.Puzzlers.Stores.APIStores
{
    public interface ICompetitionApiClient
    {
        Task<CompetitionDTO?> GetCompetitionByIdAsync(Guid id);
        Task<List<CompetitionDTO>> GetAllCompetitionsAsync();
        Task<CompetitionDTO?> GetCompetitionByNameAsync(string name);
        Task<CompetitionDTO> CreateOrUpdateAsync(CompetitionDTO competition);
    }

    public class CompetitionApiClient : ICompetitionApiClient
    {
        private readonly HttpClient m_httpClient;
        private readonly JsonSerializerOptions m_jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public CompetitionApiClient(HttpClient httpClient)
        {
            m_httpClient = httpClient;
        }

        public async Task<CompetitionDTO?> GetCompetitionByIdAsync(Guid id)
        {
            var response = await m_httpClient.GetAsync($"{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            var competition = JsonSerializer.Deserialize<CompetitionDTO>(content, m_jsonOptions);
            return competition;
        }

        public async Task<CompetitionDTO?> GetCompetitionByNameAsync(string name)
        {
            var response = await m_httpClient.GetAsync($"?name={name}");
            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            var competition = JsonSerializer.Deserialize<CompetitionDTO>(content, m_jsonOptions);
            return competition;
        }

        public async Task<List<CompetitionDTO>> GetAllCompetitionsAsync()
        {
            var response = await m_httpClient.GetAsync("");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var competitions = JsonSerializer.Deserialize<List<CompetitionDTO>>(content, m_jsonOptions);
            return competitions ?? new List<CompetitionDTO>();
        }

        public async Task<CompetitionDTO> CreateOrUpdateAsync(CompetitionDTO competition)
        {
            var json = JsonSerializer.Serialize(competition, m_jsonOptions);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await m_httpClient.PutAsync($"{competition.Id}", jsonContent);
            response.EnsureSuccessStatusCode();
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parsedCompetition = JsonSerializer.Deserialize<CompetitionDTO>(content, m_jsonOptions);
                competition= parsedCompetition ?? competition;
            }
            return competition;
        }
    }
}