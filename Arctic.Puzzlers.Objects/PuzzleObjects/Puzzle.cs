using Arctic.Puzzlers.Objects.Misc;
using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.PuzzleObjects
{
    public class Puzzle
    {
        public long ShortId { get; set; }
        [JsonConverter(typeof(BrandNameJsonConvert))]
        public BrandName BrandName { get; set; }
        public string? Name { get; set; }
        public int Size { get; set; }
    }
}
