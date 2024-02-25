using Arctic.Puzzlers.Objects.Misc;
using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class Participant
    {
        public required string FullName {  get; set; }

        [JsonConverter(typeof(CountryJsonConverter))]
        public Countries? Country { get; set; }

        public ParsedIdentifier? ParsedIdentifier { get; set; }
    }
}
