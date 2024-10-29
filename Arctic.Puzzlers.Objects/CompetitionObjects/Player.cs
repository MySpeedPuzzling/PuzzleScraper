using Arctic.Puzzlers.Objects.Misc;
using System.Text.Json.Serialization;
namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class Player
    {
        public Player()
        {
            Id= Guid.NewGuid();
        }
        public Player (Guid id)
        {
            if (id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
        }
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;


        [JsonConverter(typeof(CountryJsonConverter))]
        public Countries? Country { get; set; }

        public ParsedIdentifier? ParsedIdentifier { get; set; }
    }
}
