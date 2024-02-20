using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Arctic.Puzzlers.Stores.Filestore
{
    public class JsonFileStore : IFullDataStorage
    {
        private readonly IConfiguration m_configuration;
        public JsonFileStore(IConfiguration config) 
        {
            m_configuration = config;
        }
        private const string m_storeType = "file";
        public async Task StoreAllPuzzleData(List<Competition> competitions, List<PuzzleExtended> puzzles)
        {
            var folder = m_configuration.GetFileOutputFolder();
            var fileName = Path.Combine(folder, "data" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".json");
            await using (FileStream createStream = File.Create(fileName))
            {
                var outputdata = new OutputData() { Competitions = competitions, Puzzles = puzzles };
                await JsonSerializer.SerializeAsync(createStream, outputdata);
            }
        }

        public bool SupportedStoreType(string storeType)
        {
            return m_storeType.ToLower() == storeType.ToLower();
        }
    }
}
