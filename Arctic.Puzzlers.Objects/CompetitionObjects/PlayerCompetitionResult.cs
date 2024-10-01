namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class PlayerCompetitionResult
    {
        public ParticipantResult Result { get; set; } = new ParticipantResult();
        public string CompetitionName { get; set; } = string.Empty;
    }
}
