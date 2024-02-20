using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Arctic.Puzzlers.Stores
{
    public class FullDataStoreFactory
    {
        private IEnumerable<IFullDataStorage> m_stores;
        private ILogger<FullDataStoreFactory> m_logger;
        private IConfiguration m_configuration;

        public FullDataStoreFactory(IEnumerable<IFullDataStorage> parsers, ILogger<FullDataStoreFactory> logger, IConfiguration configuration)
        {
            m_stores = parsers;
            m_logger = logger;
            m_configuration = configuration;
        }

        public async Task StoreData( List<Competition> competitions, List<PuzzleExtended> puzzles)
        {
            var outputTypes = m_configuration.GetOutputTypes();
            if (!string.IsNullOrEmpty(outputTypes))
            {
                var outputTypeList = outputTypes.Split(';');
                foreach (IFullDataStorage store in m_stores)
                {
                    if (outputTypeList.Any(t=> store.SupportedStoreType(t.ToLower())))
                    {
                        m_logger.LogInformation($"Running store function {store.GetType()}");
                        await store.StoreAllPuzzleData(competitions, puzzles);
                    }
                    else
                    {
                        m_logger.LogInformation($"Skipping store {store.GetType()}");
                    }
                    
                }
            }
            else
            {
                m_logger.LogInformation("No output types implemented");
            }
           
        }
    }
}
