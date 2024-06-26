﻿namespace Arctic.Puzzlers.Objects.PuzzleObjects
{
    public class PuzzleExtended : Puzzle
    {
        public PuzzleExtended() 
        {
            ImageUrls = new List<string>();
        }
        public long NumberOfPieces { get; set; }
        public string EAN {  get; set; } 
        
        public string Description { get; set; }
        public List<string> ImageUrls { get; set; }

        public string Url {  get; set; }
    }
}
