namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class Competition
    {
        public Competition()
        {
            CompetitionGroups = new List<CompetitionGroup>();
        }

        public Guid CompetitionId { get; set; }
        public string? Name { get; set; }
        public List<CompetitionGroup> CompetitionGroups { get; set; }
        public DateTime Time { get; set; }
        public string? Location { get; set; }
        public string? Url { get; set; }
    }
}
