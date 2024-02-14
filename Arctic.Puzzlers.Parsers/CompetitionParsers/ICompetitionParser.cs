using Arctic.Puzzlers.Objects.CompetitionObjects;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public interface ICompetitionParser
    {
        Task<List<Competition>> Parse(string url);
        bool SupportCompetitionType(CompetitionType competitionType);
    }
}
