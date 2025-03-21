using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.DTO
{
    public class CompetitionRoundDTO
    {
        public long Version = 1;
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CompetitionGroupId { get; set; } = Guid.Empty;

        public string RoundName { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RoundType RoundType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContestType ContestType { get; set; }

        public List<Puzzle> Puzzles { get; set; } = new List<Puzzle>();
        public DateTime Time { get; set; } = new DateTime();

        public TimeSpan MaxTime { get; set; } = TimeSpan.Zero;
        public int NumberOfPieces { get; set; } = 0;
        public string Location { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
