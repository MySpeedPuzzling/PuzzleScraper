
namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public interface ICompetitionParser
    {
        Task Parse(string url);
        bool SupportCompetitionType(CompetitionType competitionType);
    }
}
