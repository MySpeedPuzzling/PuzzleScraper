using Arctic.Puzzlers.Objects.PuzzleObjects;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class Result
    {
        public TimeSpan Time { get; set; }
        public long FinishedPieces { get; set; }
        public Puzzle Puzzle { get; set; }        
    }
}
