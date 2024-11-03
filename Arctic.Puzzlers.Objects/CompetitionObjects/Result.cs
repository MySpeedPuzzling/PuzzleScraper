using Arctic.Puzzlers.Objects.PuzzleObjects;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class Result
    {
        private TimeSpan m_time = TimeSpan.Zero;
        public TimeSpan Time
        {
            get
            { return m_time; }
            set
            {
                m_time = value;
                SecondsUsed = value.TotalSeconds;
            }
        }
        public double SecondsUsed { get; set; }
        public long FinishedPieces { get; set; }
        public Puzzle Puzzle { get; set; } = new Puzzle();       
    }
}
