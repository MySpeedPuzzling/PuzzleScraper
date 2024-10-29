using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;

namespace Arctic.Puzzlers.Stores.Filestore
{
    public class OutputData
    {
        public List<Competition> Competitions { get; set; } = new List<Competition>();
        public List<PuzzleExtended> Puzzles { get; set; } = new List<PuzzleExtended>();

        public List<Player> Players { get; set; } = new List<Player>();
    }
}
