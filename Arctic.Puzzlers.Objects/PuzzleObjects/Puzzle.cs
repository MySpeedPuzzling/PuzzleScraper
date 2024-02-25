using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.PuzzleObjects
{
    public class Puzzle
    {
        public long ShortId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BrandName BrandName { get; set; }
    }
}
