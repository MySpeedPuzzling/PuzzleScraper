using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class CompetitionGroup
    {
        public CompetitionGroup() 
        {
            Rounds = new List<CompetitionRound>();
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContestType ContestType { get; set; }
        public List<CompetitionRound> Rounds { get; set; }
    }
}