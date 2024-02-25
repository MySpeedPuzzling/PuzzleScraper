
using Arctic.Puzzlers.Objects.PuzzleObjects;

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
        public bool IsQualificationRound {  get; set; }
        
        public ContestType ContestType { get; set; }
        public List<ParticipantResult> Participants { get; set; }
        public List<Puzzle> Puzzles { get; set; }
        public DateTime Time { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
    }
}
