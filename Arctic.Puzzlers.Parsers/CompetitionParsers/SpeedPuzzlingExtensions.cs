using Arctic.Puzzlers.Objects.CompetitionObjects;
using Tabula;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public static class SpeedPuzzlingExtensions
    {
        public static void AddTime(this ParticipantResult participant, IReadOnlyList<Cell> row, int timeHeader)
        {
            var timestring = row[timeHeader].GetText();
            if (TimeSpan.TryParse(timestring, out TimeSpan time))
            {
                participant.Results.Add(new Result { Time = time });
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
