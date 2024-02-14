namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class Competition
    {
        public Competition()
        {
            Participants = new List<ParticipantResult>();
        }
        public string Name { get; set; }

        public List<ParticipantResult> Participants { get; set; }
    }
}
