using Arctic.Puzzlers.Objects.Misc;

namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class Participant
    {
        public required string FullName {  get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? MiddleName { get; set; }
        
        public Countries? Country { get; set; }

        public ParsedIdentifier? ParsedIdentifier { get; set; }
    }
}
