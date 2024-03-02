using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public class SpeedPuzzlingParser : ICompetitionParser
    {
        private readonly ICompetitionStore m_store;
        private readonly HttpClient m_httpClient;
        private readonly ILogger<SpeedPuzzlingParser> m_logger;
        public SpeedPuzzlingParser(ICompetitionStore store, HttpClient client, ILogger<SpeedPuzzlingParser> logger) 
        {
            m_store = store;
            m_httpClient = client;
            m_logger = logger;
        }
        public Task Parse(string url)
        {
            return Task.FromResult(0);
        }

        public async Task ParsePdf(string url)
        {
            try
            {
                var competition = new Competition();
                
                if (!await m_store.NeedToParse(url))
                {
                    return;
                }
                competition.Url = url;
                competition.Location = "Virtual";

                var response = await m_httpClient.GetAsync(url);
                using (var stream = await response.Content.ReadAsStreamAsync())
                {


                    using (var pdf = PdfDocument.Open(stream))
                    {
                        foreach (var page in pdf.GetPages())
                        {
                            // Either extract based on order in the underlying document with newlines and spaces.
                            var text = ContentOrderTextExtractor.GetText(page);
                            using (var reader = new StringReader(text))
                            {
                                competition.Name = reader.ReadLine()?.Replace("Results","").TrimEnd();
                            }
                        }

                    }
                }
                await m_store.Store(competition);
            }
            catch (Exception ex)
            {
                m_logger.LogInformation(ex, $"Error parsing {url}");

            }

        }

        public bool SupportCompetitionType(CompetitionOwner competitionType)
        {
            return competitionType == CompetitionOwner.SpeedPuzzling;
        }
    }
}
