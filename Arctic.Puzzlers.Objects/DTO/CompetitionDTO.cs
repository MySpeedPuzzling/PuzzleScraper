namespace Arctic.Puzzlers.Objects.DTO
{
    public class CompetitionDTO
    {
        public long Version = 1;
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;
        public string UniqueName { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string? Location { get; set; }
        public string? Url { get; set; }
    }
}
