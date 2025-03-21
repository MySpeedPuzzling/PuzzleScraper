using Arctic.Puzzlers.Objects.CompetitionObjects;
using System.Text.Json.Serialization;
namespace Arctic.Puzzlers.Objects.DTO
{
    public class CompetitionGroupDTO
    {
        public long Version = 1;
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CompetitionId { get; set; } = Guid.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContestType ContestType { get; set; }
    }
}
