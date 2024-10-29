using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Unicode;

namespace Arctic.Puzzlers.Stores.Filestore
{
    public class JsonPlayerStore : IPlayerStore, IDisposable
    {
        private readonly IConfiguration m_configuration;
        private const string JsonFileName = "playerdata.json";
        private List<Player> m_players;
        private JsonSerializerOptions m_serializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public JsonPlayerStore(IConfiguration config)
        {
            m_players = new List<Player>();
            m_configuration = config;
            Init();
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
                    if (outputdata?.Players != null)
                    {
                        m_players = outputdata.Players;
                    }
                }
            }
        }
        public Task<IEnumerable<Player>> FindPlayerByName(string name)
        {
            var players = m_players.Where(p => p.FullName.Equals(name,StringComparison.InvariantCultureIgnoreCase));
            return Task.FromResult(players);
        }

        public  Task<Player?> GetPlayerByGuid(Guid id)
        {
            var player = m_players.FirstOrDefault(p=> p.Id == id);
            return Task.FromResult(player);
        }

        public Task<bool> Store(Player player)
        {
            if (!m_players.Any(t => t.FullName == player.FullName))
            {
                m_players.Add(player);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public void Dispose()
        {
            var folder = m_configuration.GetFileOutputFolder();
            var fileName = Path.Combine(folder, JsonFileName);

            using (FileStream createStream = File.Create(fileName))
            {
                var outputdata = new OutputData() { Players = m_players };
                JsonSerializer.Serialize(createStream, outputdata, m_serializeOptions);
            }
        }
    }
}
