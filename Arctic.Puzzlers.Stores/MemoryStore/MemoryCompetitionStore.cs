using Arctic.Puzzlers.Objects.CompetitionObjects;
using Microsoft.Extensions.Configuration;


namespace Arctic.Puzzlers.Stores.MemoryStore
{
    public class MemoryCompetitionStore : ICompetitionStore
    {
        private readonly IConfiguration m_configuration;
        private List<Competition> m_competitionList;

        public MemoryCompetitionStore(IConfiguration config)
        {
            m_competitionList = new List<Competition>();
            m_configuration = config;
        }

        public Task<bool> Store(Competition competition)
        {
            if (m_configuration.OverrideData())
            {
                m_competitionList.RemoveAll(t => t.Url == competition.Url);
                m_competitionList.Add(competition);
                return Task.FromResult(true);
            }
            else if (!m_competitionList.Any(t => t.Url == competition.Url))
            {
                m_competitionList.Add(competition);
                Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> NeedToParse(string url)
        {
            return Task.FromResult(!m_competitionList.Any(t => url == t.Url) || m_configuration.OverrideData());
        }

        public Task<List<Competition>> GetAll()
        {
            return Task.FromResult(m_competitionList);
        }
    }
}
