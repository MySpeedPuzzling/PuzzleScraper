using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class ParsedIdentifier
    {
        public string? OriginalUrl {  get; set; }
        public string? Identifier { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CompetitionOwner UserOwner { get; set; }
    }
}
