using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class CompetitionGroup
    {
        public CompetitionGroup() { }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContestType ContestType { get; set; }
        public List<CompetitionRound> QualificationRounds { get; set; }
        public List<CompetitionRound> Semifinals { get; set; }
        public CompetitionRound Final { get;set;}
    }
}