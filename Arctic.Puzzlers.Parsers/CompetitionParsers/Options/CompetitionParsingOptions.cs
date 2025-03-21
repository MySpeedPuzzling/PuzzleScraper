using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers.Options
{
    public class CompetitionParsingOptions 
    {
        public required string LocalStoragePath { get; set; }
    }

    public class CompetitionParsingOptionsSetup : IConfigureOptions<CompetitionParsingOptions>
    {
        private const string CompetitionDataFolderKey = "COMPETITIONDATAFOLDER";
        private readonly IConfiguration _configuration;

        public CompetitionParsingOptionsSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(CompetitionParsingOptions options)
        {
            options.LocalStoragePath = _configuration[CompetitionDataFolderKey] ?? string.Empty;
        }      

    }
}
