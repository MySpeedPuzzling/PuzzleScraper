using Arctic.Puzzlers.Objects.CompetitionObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Arctic.Puzzlers.Stores.Filestore
{
    public class JsonCompetitionStore :  ICompetitionStore, IDisposable
    {
        private readonly IConfiguration m_configuration;
        private readonly ILogger<JsonCompetitionStore> m_logger;
        private const string JsonFileName = "competitiondata.json";
        private List<Competition> m_competitionList;
        private JsonSerializerOptions m_serializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public JsonCompetitionStore(IConfiguration config, ILogger<JsonCompetitionStore> logger) 
        {
            m_competitionList = new List<Competition>();
            m_configuration = config;
            m_logger = logger;
            Init();
           
        }      

        public void Init()
        {
            var folder = m_configuration.GetFileOutputFolder();
            var fileName = Path.Combine(folder, JsonFileName);
            m_logger.LogInformation($"Trying to load competitions from json file {fileName}");
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

        public Task<bool> Store(Competition competition)
        {
            if (m_configuration.OverrideData())
            {
                m_competitionList.RemoveAll(t => t.Url == competition.Url);
                m_competitionList.Add(competition);
                return Task.FromResult(true);
            }
            else if (!m_competitionList.Any(t => t.Url == competition.Url))
            {
                m_competitionList.Add(competition);
                Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> NeedToParse(string url)
        {
            return Task.FromResult(!m_competitionList.Any(t => url == t.Url) || m_configuration.OverrideData());
        }

        public Task<List<Competition>> GetAll()
        {
            return Task.FromResult(m_competitionList);
        }

        public Task<List<PlayerCompetitionResult>> GetPlayerCompetitionResultByName(string name)
        {
            List<PlayerCompetitionResult> results = new List<PlayerCompetitionResult>();
            foreach (var competition in m_competitionList)
            {
                var competitionName = competition.Name;
                foreach(var group in competition.CompetitionGroups)
                {
                    var contesttype = group.ContestType.ToString();
                    foreach(var round in group.Rounds)
                    {
                        var roundname = round.RoundName;
                        foreach (var participantResult in round.Participants.Where(t => t.Participants.Any(p => p.FullName.ToLower() == name.ToLower())))
                        {
                            results.Add(new PlayerCompetitionResult { CompetitionName = competitionName + " " + contesttype + " " + roundname, Result = participantResult });
                        }
                    }
                }
            }
            return Task.FromResult(results);
        }

        public Task<List<Competition>> GetByName(string name)
        {
            var results = m_competitionList.Where(t=> t.Name != null && t.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase)).ToList();
            return Task.FromResult(results);
        }
    }
}
