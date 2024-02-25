using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Configuration;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Arctic.Puzzlers.Stores.Filestore
{
    public class JsonCompetitionStore :  ICompetitionStore
    {
        private readonly IConfiguration m_configuration;
        private const string JsonFileName = "competitiondata.json";
        private const string m_storeType = "file";
        private List<CompetitionRound> m_competitionList;
        private JsonSerializerOptions m_serializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),

        };
        public JsonCompetitionStore(IConfiguration config) 
        {
            m_competitionList = new List<CompetitionRound>();
            m_configuration = config;
            Init();
           
        }      

        public bool SupportedStoreType(string storeType)
        {
            return m_storeType.ToLower() == storeType.ToLower();
        }

        public void Init()
        {
            var folder = m_configuration.GetFileOutputFolder();
            var fileName = Path.Combine(folder, JsonFileName);
            if (File.Exists(fileName))
            {
                
                using (FileStream createStream = File.OpenRead(fileName))
                {
                    var outputdata = JsonSerializer.Deserialize<OutputData>(createStream, m_serializeOptions);
                    if (outputdata?.Competitions != null)
                    {
                        m_competitionList = outputdata.Competitions;
                    }
                }
            }            
        }
        
        public void Dispose()
        {
            var folder = m_configuration.GetFileOutputFolder();
            var fileName = Path.Combine(folder, JsonFileName);      

            using (FileStream createStream = File.Create(fileName))
            {
                var outputdata = new OutputData() { Competitions = m_competitionList };
                JsonSerializer.Serialize(createStream, outputdata, m_serializeOptions);
            }
        }


        public Task<bool> Store(CompetitionRound competition)
        {
            if (m_configuration.OverrideData())
            {
                m_competitionList.RemoveAll(t=> t.Url == competition.Url);
                m_competitionList.AddCompetitionIfNew(competition);
                return Task.FromResult(true);
            }
            var isAdded = m_competitionList.AddCompetitionIfNew(competition);
            return Task.FromResult(isAdded);
        }

        public Task<bool> NeedToParse(string url)
        {
            return Task.FromResult(!m_competitionList.Any(t => url == t.Url) || m_configuration.OverrideData());
        }
    }
}
