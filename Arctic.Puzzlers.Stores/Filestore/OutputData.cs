using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;

namespace Arctic.Puzzlers.Stores.Filestore
{
    internal class OutputData
    {
        public List<Competition> Competitions { get; set; }    
        public List<PuzzleExtended> Puzzles { get; set; }
    }
}
