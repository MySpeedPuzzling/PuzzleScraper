using Arctic.Puzzlers.Objects.CompetitionObjects;

namespace Arctic.Puzzlers.Stores.Filestore
{
    public static class CompetitionExtension
    {
        public static bool AddCompetitionIfNew(this List<CompetitionRound> competitions, CompetitionRound newCompetition)
        {
            if (competitions.Any(t =>
                t.Time == newCompetition.Time &&
                t.ContestType == newCompetition.ContestType &&
                t.RoundName == newCompetition.RoundName &&
                t.Participants.Count() == newCompetition.Participants.Count()))
            {
                return false;
            }
            else
            {
                competitions.Add(newCompetition);
                return true;
            }
        }
    }
}
