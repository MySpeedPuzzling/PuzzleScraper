using Arctic.Puzzlers.Objects.CompetitionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    internal static class CompetitionRoundExtensions
    {
        internal static void SetContestType(this CompetitionRound competitionObject)
        {
            if (competitionObject.Participants.All(t => t.Participants.Count <= 1))
            {
                competitionObject.ContestType = ContestType.Individual;
            }
            else if (competitionObject.Participants.All(t => t.Participants.Count <= 2))
            {
                competitionObject.ContestType = ContestType.Pair;
            }
            else if (competitionObject.Participants.All(t => t.Participants.Count <= 4))
            {
                competitionObject.ContestType = ContestType.Team;
            }
            else
            {
                competitionObject.ContestType = ContestType.Unknown;
            }
        }
    }
}
