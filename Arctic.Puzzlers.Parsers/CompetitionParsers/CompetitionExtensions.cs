using Arctic.Puzzlers.Objects.CompetitionObjects;
using System.Text;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public static class CompetitionExtensions
    {
        public static void SetTotalResults(this Competition competition)
        {
            competition.CompetitionGroups.ForEach(t => t.Rounds.ForEach(
                k =>
                { 
                    k.Participants.ForEach(participant => SetResults(participant, k));
                    if (k.MaxTime == TimeSpan.Zero)
                    {
                        k.MaxTime = GuessMaxTime(k);
                    }
                }));
        }

        private static TimeSpan GuessMaxTime(CompetitionRound round)
        {
            var highestTime = round?.Participants?.OrderBy(t => t.TotalTime)?.Last()?.TotalTime;
            if (highestTime.HasValue)
            {
                int minutes = highestTime.Value.Minutes < 30 ? 30 : 0;
                int hours = highestTime.Value.Minutes < 30 ? highestTime.Value.Hours : highestTime.Value.Hours + 1;
                return new TimeSpan(hours: hours, minutes: minutes, 0);
            }
            return new TimeSpan(hours: 2, minutes: 0, 0);
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
