using Arctic.Puzzlers.Objects.DTO;
using Tabula;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public static class SpeedPuzzlingExtensions
    {
        public static void AddResult(this ResultDTO participant, IReadOnlyList<Cell> row, int timeHeader, int piecesPerMinuteHeader)
        {
            var timestring = row[timeHeader].GetText();
            if (TimeSpan.TryParse(timestring, out TimeSpan time))
            {
                var result= new PuzzleResult { PuzzleTime = time };

                var piecesPerMinuteString = row[piecesPerMinuteHeader].GetText();
                if (double.TryParse(piecesPerMinuteString, out double piecesPerMinute))
                {
                    result.ParsedPiecesPerMinute = piecesPerMinute;
                }
                participant.Results.Add(result);
            }           
        }

        public static void AddRank(this ResultDTO participant, IReadOnlyList<Cell> row, int rankHeader)
        {
            var rankstring = row[rankHeader]?.TextElements?.First()?.GetText();
            if (int.TryParse(rankstring, out int rank))
            {
                participant.Rank = rank;
            }
        }

        public static string FixName(this string name)
        {
            var nameParts = name.Split(",");
            var lastName = nameParts.First().Trim();
            var firstname = nameParts.Last().Trim();
            return firstname + " " + lastName;
        }

        public static string[] ReadAndSplit(this StringReader reader, char split)
        {
            var line = reader.ReadLine();
            if(line == null)
            {
                return new string[0];
            }
            return line.Split(split);
        }
        
    }
}
