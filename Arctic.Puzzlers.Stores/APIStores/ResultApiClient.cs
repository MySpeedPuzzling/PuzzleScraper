using System.Net;
using System.Text;
using System.Text.Json;
using Arctic.Puzzlers.Objects.DTO;

namespace Arctic.Puzzlers.Stores.APIStores
{
    public interface IResultApiClient
    {
        Task<ResultDTO?> GetByIdAsync(Guid id);
        Task<ResultDTO> CreateOrUpdateAsync(ResultDTO result);
    }

    public class ResultApiClient : IResultApiClient
    {
        private readonly HttpClient m_httpClient;
        private readonly JsonSerializerOptions m_jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ResultApiClient(HttpClient httpClient)
        {
            m_httpClient = httpClient;
        }


        public async Task<ResultDTO?> GetByIdAsync(Guid id)
        {
            var response = await m_httpClient.GetAsync($"{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResultDTO>(content, m_jsonOptions);
        }

        public async Task<ResultDTO> CreateOrUpdateAsync(ResultDTO result)
        {
            var json = JsonSerializer.Serialize(result, m_jsonOptions);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            // Assumes PUT endpoint at api/Result/{id}
            var response = await m_httpClient.PutAsync($"{result.Id}", jsonContent);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parsedResult = JsonSerializer.Deserialize<ResultDTO>(content, m_jsonOptions);
                result = parsedResult ?? result;
            }
            return result;
        }
    }
}