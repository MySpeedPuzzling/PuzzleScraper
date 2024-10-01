
using Arctic.Puzzlers.Objects.PuzzleObjects;
using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class CompetitionRound
    {
        public string RoundName { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RoundType RoundType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContestType ContestType { get; set; }
        public List<ParticipantResult> Participants { get; set; } = new List<ParticipantResult>();
        public List<Puzzle> Puzzles { get; set; } = new List<Puzzle>();
        public DateTime Time { get; set; } = new DateTime();
        public string Location { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
