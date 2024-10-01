using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Stores;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig;
using Microsoft.Extensions.Logging;
using Tabula.Detectors;
using Tabula.Extractors;
using Tabula;
using Arctic.Puzzlers.Objects.Misc;
using HtmlAgilityPack;
using Arctic.Puzzlers.Objects.PuzzleObjects;

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
        public async Task Parse(string url)
        {
            var web = new HtmlWeb();
            var mainPage = web.Load(url);
            var results = mainPage.DocumentNode.SelectNodes("//a[contains(@href,'results.pdf')]");
            if (!results.Any())
            {
                m_logger.LogInformation($"Could not parse any data from {url}");
                return;
            }
            foreach (var result in results)
            {
                string competitionUrl = string.Empty;
                try
                {
                    var relativurl = result.GetAttributeValue("href", string.Empty);
                    if (string.IsNullOrEmpty(relativurl))
                    {
                        continue;
                    }
                    competitionUrl = GetBaseUrl(url) + relativurl;
                    if (!await m_store.NeedToParse(competitionUrl))
                    {
                        continue;
                    }
                    var response = await m_httpClient.GetAsync(competitionUrl);
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var competition = new Competition();
                        competition.Url = competitionUrl;
                        competition.Location = "Virtual";

                        var competitionGroup = ParsePdf(stream);
                        competition.CompetitionGroups.Add(competitionGroup);
                        var stored = await m_store.Store(competition);
                        if (stored)
                        {
                            m_logger.LogInformation($"Stored competition from pdf {url}");
                        }
                    }
                }               
                catch (Exception ex)
                {
                    m_logger.LogInformation(ex, $"Error parsing {competitionUrl}");

                }
            }
        }

        public CompetitionGroup ParsePdf(Stream stream)
        {            
            var competitionGroup = new CompetitionGroup();
            var competitionRound = new CompetitionRound();

            using (var pdf = PdfDocument.Open(stream, new ParsingOptions() { ClipPaths = true }))
            {
                var rows = GetTableRows(pdf);
                foreach (var page in pdf.GetPages())
                {
                    var text = ContentOrderTextExtractor.GetText(page);
                    var textLines = text.Split(new string[] { "\r\n", "\r", "\n" },StringSplitOptions.None);

                    var firstLine = textLines.First();
                    competitionRound.RoundName = firstLine.Replace("Results", "").TrimEnd();
                    string puzzleName = textLines.Last();
                    string puzzleBrand = textLines[textLines.Length - 2];
                    puzzleBrand = WashPuzzleBrandName(puzzleBrand);
                    BrandName puzzleBrandEnum = puzzleBrand.GetEnumFromString<BrandName>();

                    var topRow = rows.First().ToArray();
                    var timeHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("time"));
                    var nameHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("name"));
                    var countryHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("location"));
                    switch (text.ToLower())
                    {
                        case string a when a.Contains("team"):
                            competitionRound.ContestType = ContestType.Teams;
                            competitionGroup.ContestType = ContestType.Teams;
                            AddResultForTeam(competitionRound, rows, timeHeader, nameHeader, countryHeader);
                            break;
                        case string b when b.Contains("pair"):
                            competitionRound.ContestType = ContestType.Pairs;
                            competitionGroup.ContestType = ContestType.Pairs;
                            AddResultForPairs(competitionRound, rows, timeHeader, nameHeader, countryHeader);
                            break;
                        case string c when c.Contains("solo"):
                        case string d when d.Contains("individual"):
                            competitionRound.ContestType = ContestType.Individual;
                            competitionGroup.ContestType = ContestType.Individual;
                            AddResultForIndividual(competitionRound, rows, timeHeader, nameHeader, countryHeader);
                            break;
                        default:
                            competitionRound.ContestType = ContestType.Individual;
                            competitionGroup.ContestType = ContestType.Individual;
                            AddResultForIndividual(competitionRound, rows, timeHeader, nameHeader, countryHeader);
                            break;
                    }

                    competitionGroup.Rounds.ForEach(t => 
                    { 
                        t.Participants.ForEach(k => k.Results.ForEach(m => { m.Puzzle.BrandName = puzzleBrandEnum; m.Puzzle.Name = puzzleName; }));
                        t.Puzzles.Add(new Puzzle { BrandName = puzzleBrandEnum, Name = puzzleName });
                    });

                    
                }
            }

            competitionGroup.Rounds.Add(competitionRound);

            return competitionGroup;             
           
        }

        private string WashPuzzleBrandName(string puzzleBrand)
        {
            var puzzleBrandParts = puzzleBrand.Split(' ');
            var returnValue = string.Empty;
            foreach(var  part in puzzleBrandParts)
            {
                if (char.IsDigit(part[0]))
                {
                    break;
                }
                returnValue += part;
            }

            return returnValue;
        }

        private static void AddResultForTeam(CompetitionRound competitionRound, IReadOnlyList<IReadOnlyList<Cell>> rows, int timeHeader, int nameHeader, int countryHeader)
        {
            var participant = new ParticipantResult();
            var country = Countries.UNK;
            int i = 0;
            foreach (var row in rows.Skip(1))
            {
                i++;
                if (i == 3)
                {
                    participant.AddTime(row, timeHeader);
                    var countryString = row[countryHeader].GetText();
                    country = countryString.GetEnumFromString<Countries>();
                    if (country == Countries.UNK)
                    {
                        country = Countries.USA;
                    }
                }
                if (i != 3)
                {
                    participant.AddParticipant(row, nameHeader, countryHeader);
                }
                if(i ==5)
                {
                    participant = new ParticipantResult();
                    country = Countries.UNK;
                    i = 0;
                    competitionRound.Participants.Add(participant);
                }
                
            }
        }
        private static void AddResultForPairs(CompetitionRound competitionRound, IReadOnlyList<IReadOnlyList<Cell>> rows, int timeHeader, int nameHeader, int countryHeader)
        {
            foreach (var row in rows.Skip(1))
            {
                var participant = new ParticipantResult();
                participant.AddTime(row, timeHeader);

                participant.AddPairParticipants(row, nameHeader, countryHeader);

                competitionRound.Participants.Add(participant);
            }
        }
        private static void AddResultForIndividual(CompetitionRound competitionRound, IReadOnlyList<IReadOnlyList<Cell>> rows, int timeHeader, int nameHeader, int countryHeader)
        {
            foreach (var row in rows.Skip(1))
            {
                var participant = new ParticipantResult();
                participant.AddTime(row, timeHeader);

                participant.AddParticipant(row, nameHeader, countryHeader);

                competitionRound.Participants.Add(participant);
            }
        }

        private static IReadOnlyList<IReadOnlyList<Cell>> GetTableRows(PdfDocument pdf)
        {
            IReadOnlyList<IReadOnlyList<Cell>> rows = new List<IReadOnlyList<Cell>>();
            ObjectExtractor oe = new ObjectExtractor(pdf);
            for (int i = 1; i <= pdf.NumberOfPages; i++)
            {
                PageArea tablePage = oe.Extract(1);
                // detect canditate table zones
                SimpleNurminenDetectionAlgorithm detector = new SimpleNurminenDetectionAlgorithm();
                var regions = detector.Detect(tablePage);

                IExtractionAlgorithm ea = new BasicExtractionAlgorithm();
                List<Table> tables = ea.Extract(tablePage.GetArea(regions[0].BoundingBox)); // take first candidate area
                var table = tables[0];
                rows = table.Rows;
            }
            return rows;
        }

        public static string GetBaseUrl(string url)
        {
            var uri = new Uri(url);
            var baseUri = uri.GetLeftPart(System.UriPartial.Authority);
            return baseUri;
        }

        public bool SupportCompetitionType(CompetitionOwner competitionType)
        {
            return competitionType == CompetitionOwner.SpeedPuzzling;
        }
    }
}
