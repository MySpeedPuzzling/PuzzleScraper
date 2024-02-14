namespace Arctic.Puzzlers.Objects.PuzzleObjects
{
    public class PuzzleExtended : Puzzle
    {
        public long NumberOfPieces { get; set; }
        public string InternalIdentifier { get; set; }
        public string Identifier {  get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
