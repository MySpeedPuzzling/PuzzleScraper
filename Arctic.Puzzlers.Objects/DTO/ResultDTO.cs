using Arctic.Puzzlers.Objects.PuzzleObjects;

namespace Arctic.Puzzlers.Objects.DTO
{
    public class ResultDTO
    {
        public readonly long Version = 1;

        public Guid Id { get; set; } = Guid.NewGuid();
        public List<Guid> PlayerIds { get; set; } = new List<Guid>();

        public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;

        public long FinishedPieces { get; set; } = 0;

        public Guid CompetitionRoundId { get; set; } = Guid.Empty;

        public string? GroupName { get; set; }

        public bool? Qualified { get; set; }

        public int Rank { get; set; } = 0;

        public List<PuzzleResult> Results {get;set; } = new List<PuzzleResult>();
    }

    public class PuzzleResult
    {
        public TimeSpan PuzzleTime { get; set; } = TimeSpan.Zero;
        public long FinishedPieces { get; set; }
        public double? ParsedPiecesPerMinute { get; set; }
        public Puzzle Puzzle { get; set; } = new Puzzle();
    }
}
