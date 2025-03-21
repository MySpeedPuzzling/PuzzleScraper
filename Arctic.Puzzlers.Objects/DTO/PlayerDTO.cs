using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.Misc;
using System.Text.Json.Serialization;
namespace Arctic.Puzzlers.Objects.DTO
{
    public class PlayerDTO
    {
        public PlayerDTO()
        {
            Id= Guid.NewGuid();
        }
        public PlayerDTO (Guid id)
        {
            if (id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
        }

        public const long ObjectVersion = 1;
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;


        [JsonConverter(typeof(CountryJsonConverter))]
        public Countries? Country { get; set; }

        public List<ParsedIdentifier> ParsedIdentifier { get; set; } = new List<ParsedIdentifier>();
    }
}
