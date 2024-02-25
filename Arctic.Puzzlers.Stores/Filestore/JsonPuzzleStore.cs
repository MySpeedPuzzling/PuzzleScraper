using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Configuration;
using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Arctic.Puzzlers.Stores.Filestore
{
    public class JsonPuzzleStore : IPuzzleStore
    {
        private readonly IConfiguration m_configuration;
        private const string JsonFileName = "puzzledata.json";
        private const string m_storeType = "file";
        private List<PuzzleExtended> m_puzzleList;
        private JsonSerializerOptions m_serializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),

        };

        public JsonPuzzleStore(IConfiguration config) 
        {
            m_puzzleList = new List<PuzzleExtended>();
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
                    if (outputdata?.Puzzles != null)
                    {
                        m_puzzleList = outputdata.Puzzles;
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
                var outputdata = new OutputData() { Puzzles = m_puzzleList };
                JsonSerializer.Serialize(createStream, outputdata, m_serializeOptions);
            }
        }

        public Task<bool> Store(PuzzleExtended puzzle)
        {
            if(!m_puzzleList.Any(t=> puzzle.BrandName == t.BrandName && puzzle.ShortId == t.ShortId))
            {
                m_puzzleList.Add(puzzle);
                return Task.FromResult(true);
            }
            if (m_configuration.OverrideData())
            {
                m_puzzleList.RemoveAll(t => puzzle.BrandName == t.BrandName && puzzle.ShortId == t.ShortId);
                m_puzzleList.Add(puzzle);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);            
        }

        public Task<bool> NeedToParse(BrandName brandName, long ShortId)
        {
            return Task.FromResult(!m_puzzleList.Any(t => brandName == t.BrandName && ShortId == t.ShortId) || m_configuration.OverrideData());            
        }

        public Task<bool> NeedToParse(string url)
        {
            return Task.FromResult(!m_puzzleList.Any(t => url == t.Url) || m_configuration.OverrideData());
        }
    }
}
