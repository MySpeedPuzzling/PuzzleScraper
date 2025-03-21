using Arctic.Puzzlers.Objects.DTO;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
namespace Arctic.Puzzlers.Stores.APIStores
{

    public class PlayerApiClient : IPlayerStore
    {
        private readonly HttpClient m_httpClient;
        private readonly ILogger<PlayerApiClient> m_logger;

        public PlayerApiClient(HttpClient httpClient, ILogger<PlayerApiClient> logger)
        {
            m_httpClient = httpClient;
            m_logger = logger;
        }

        public async Task<IEnumerable<PlayerDTO>?> GetPlayersByNameAsync(string name)
        {
            try
            {
                var response = await m_httpClient.GetAsync($"?name={name}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<PlayerDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to get players by name");
                throw;
            }
        }

        public async Task<PlayerDTO?> GetPlayerByIdAsync(Guid id)
        {
            try
            {
                var response = await m_httpClient.GetAsync($"{id}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PlayerDTO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to get player by id");
                throw;
            }

        }

        public async Task<bool> CreateOrUpdatePlayerAsync(Guid id, PlayerDTO player)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(player), Encoding.UTF8, "application/json");
                var response = await m_httpClient.PutAsync($"{id}", jsonContent);

                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to create or update player");
                throw;
            }
        }
    }

}
