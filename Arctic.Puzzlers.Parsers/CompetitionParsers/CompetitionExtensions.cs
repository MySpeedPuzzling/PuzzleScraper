using Arctic.Puzzlers.Objects.CompetitionObjects;
using System.Text;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public static class CompetitionExtensions
    {
        public static void SetTotalResults(this Competition competition)
        {
            competition.CompetitionGroups.ForEach(t => t.Rounds.ForEach(k => k.Participants.ForEach(participant => SetResults(participant,k))));            
        }
        private static void SetResults(ParticipantResult participant, CompetitionRound round)
        {
            TimeSpan totalResults = TimeSpan.Zero;
            long finishedPieces = 0;
            foreach (var result in participant.Results)
            {
                totalResults = totalResults.Add(result.Time);
                finishedPieces += result.FinishedPieces;
            }
            participant.FinishedPieces = finishedPieces == 0 ? round.NumberOfPieces : finishedPieces;

            participant.TotalTime = totalResults;
        }
    }
}
