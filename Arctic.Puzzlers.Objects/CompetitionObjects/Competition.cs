
using Arctic.Puzzlers.Objects.PuzzleObjects;
using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class Competition
    {
        public Competition()
        {
            Participants = new List<ParticipantResult>();
            Puzzles = new List<Puzzle>();
        }
        public string Name { get; set; }
        public ContestType ContestType { get; set; }
        public List<ParticipantResult> Participants { get; set; }
        public List<Puzzle> Puzzles { get; set; }
        public DateTime Time { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
    }
}
