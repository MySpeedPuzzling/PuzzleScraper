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
using System.Collections;
using System.Reflection.Metadata;
using Tabula.Detectors;
using Tabula.Extractors;
using Tabula;

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
                var competitionGroup = new CompetitionGroup();
                var competitionRound = new CompetitionRound();
                var response = await m_httpClient.GetAsync(url);
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var pdf = PdfDocument.Open(stream, new ParsingOptions() { ClipPaths = true }))
                    {
                        var rows = GetTableRows(pdf);
                        foreach (var page in pdf.GetPages())
                        {
                            var text = ContentOrderTextExtractor.GetText(page);
                            using (var reader = new StringReader(text))
                            {
                                competition.Name = reader.ReadLine()?.Replace("Results", "").TrimEnd();
                                // Move to line with headers
                                var topRow = rows.First().ToArray();
                                var timeHeader = Array.FindIndex(topRow, t=> t.GetText().ToLower().Contains("time"));
                                var nameHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("name"));
                                var countryHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("location"));
                                foreach(var row in rows.Skip(1))
                                {
                                    var participant = new ParticipantResult();
                                    participant.AddTime(row, timeHeader);

                                    participant.AddParticipant(row, nameHeader, countryHeader);

                                    competitionRound.Participants.Add(participant);
                                }
                            }
                        }

                    }
                }
                competitionRound.SetContestType();
                competitionGroup.ContestType = competitionRound.ContestType;
                competitionGroup.Rounds.Add(competitionRound);
                competition.CompetitionGroups.Add(competitionGroup);

                var stored = await m_store.Store(competition);
                if (stored)
                {
                    m_logger.LogInformation($"Stored competition from pdf {url}");
                }
            }
            catch (Exception ex)
            {
                m_logger.LogInformation(ex, $"Error parsing {url}");

            }

        }

        private static IReadOnlyList<IReadOnlyList<Cell>> GetTableRows(PdfDocument pdf)
        {
            ObjectExtractor oe = new ObjectExtractor(pdf);
            PageArea tablePage = oe.Extract(1);

            // detect canditate table zones
            SimpleNurminenDetectionAlgorithm detector = new SimpleNurminenDetectionAlgorithm();
            var regions = detector.Detect(tablePage);

            IExtractionAlgorithm ea = new BasicExtractionAlgorithm();
            List<Table> tables = ea.Extract(tablePage.GetArea(regions[0].BoundingBox)); // take first candidate area
            var table = tables[0];
            var rows = table.Rows;
            return rows;
        }


        public bool SupportCompetitionType(CompetitionOwner competitionType)
        {
            return competitionType == CompetitionOwner.SpeedPuzzling;
        }
    }
}
