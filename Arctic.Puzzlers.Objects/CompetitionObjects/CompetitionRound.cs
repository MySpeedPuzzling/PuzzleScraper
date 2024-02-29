
using Arctic.Puzzlers.Objects.PuzzleObjects;
using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class CompetitionRound
    {
        public CompetitionRound()
        {
            Participants = new List<ParticipantResult>();
            Puzzles = new List<Puzzle>();
        }
        public string RoundName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RoundType RoundType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContestType ContestType { get; set; }
        public List<ParticipantResult> Participants { get; set; }
        public List<Puzzle> Puzzles { get; set; }
        public DateTime Time { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
    }
}
