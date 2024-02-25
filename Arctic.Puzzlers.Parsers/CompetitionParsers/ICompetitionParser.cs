
using Arctic.Puzzlers.Objects.CompetitionObjects;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public interface ICompetitionParser
    {
        Task Parse(string url);
        bool SupportCompetitionType(CompetitionOwner competitionType);
    }
}
