namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class ParsedIdentifier
    {
        public string? OriginalUrl {  get; set; }
        public string? Identifier { get; set; }
        public CompetitionOwner UserOwner { get; set; }
    }
}
