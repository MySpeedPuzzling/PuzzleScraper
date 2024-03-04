using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.Misc;
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

        public static void AddParticipant(this ParticipantResult participant, IReadOnlyList<Cell> row, int nameHeader, int countryHeader)
        {            
            var fullname = row[nameHeader].GetText();
            var countryString = row[countryHeader].GetText();
            var country = countryString.GetEnumFromString<Countries>();
            if(country == Countries.UNK)
            {
                country = Countries.USA;
            }
            participant.Participants.Add(new Participant { FullName = fullname, Country = country });
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
