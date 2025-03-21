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
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Arctic.Puzzlers.Parsers.CompetitionParsers.Options;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using Arctic.Puzzlers.Objects.DTO;
using Arctic.Puzzlers.Stores.APIStores;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public class SpeedPuzzlingParser : ICompetitionParser
    {
        
        private readonly HttpClient m_httpClient;
        private readonly ILogger<SpeedPuzzlingParser> m_logger;
        private readonly IPlayerStore m_playerStore;
        private readonly IOptions<CompetitionParsingOptions> m_options;
        private readonly IResultApiClient m_resultApiClient;
        private readonly ICompetitionApiClient m_competitionApiClient;
        private readonly ICompetitionGroupApiClient m_competitionGroupApiClient;
        private readonly ICompetitionRoundApiClient m_competitionRoundApiClient;

        public SpeedPuzzlingParser(HttpClient client,
                                   IResultApiClient resultApiClient,
                                   ICompetitionApiClient competitionApiClient,
                                   ICompetitionGroupApiClient competitionGroupApiClient,
                                   ICompetitionRoundApiClient competitionRoundApiClient,
                                   ILogger<SpeedPuzzlingParser> logger,
                                   IPlayerStore playerStore,
                                   IOptions<CompetitionParsingOptions> options) 
        {
            m_httpClient = client;
            m_logger = logger;
            m_playerStore = playerStore;
            m_options = options;
            m_resultApiClient = resultApiClient;
            m_competitionApiClient = competitionApiClient;
            m_competitionGroupApiClient = competitionGroupApiClient;
            m_competitionRoundApiClient = competitionRoundApiClient;
        }
        public async Task Parse(string url)
        {
            var web = new HtmlWeb();
            var mainPage = web.Load(url);
            var results = mainPage.DocumentNode.SelectNodes("//a[contains(@href,'results')]").Where(t=> t.GetAttributeValue("href",string.Empty).Contains("pdf"));
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
                                       
                    using (var stream = await GetCompetitionStream(competitionUrl))
                    {
                        
                        var competition = new CompetitionDTO();
                        competition.Url = competitionUrl;
                        competition.Location = "Virtual";
                        competition.UniqueName = relativurl;
                        competition.Name = GetCompetitionName(stream);

                      
                        var competitionDate = GetCompetitionDate(result, competitionUrl);
                        competition.Time = competitionDate;
                        var competitionFromStore = await m_competitionApiClient.CreateOrUpdateAsync(competition);
                        if (competitionFromStore != null)
                        {
                            competition = competitionFromStore;
                        }
                        await ParsePdf(stream, competitionDate, competition.Id);                      
                    }
                }
                catch (Exception ex)
                {
                    m_logger.LogWarning(ex, $"Error parsing {competitionUrl}");
                }
            }
        }

        private DateTime GetCompetitionDate(HtmlNode? node, string competitionUrl)
        {
            var competitionDateTime = new DateTime();
            try
            {
                var timeNode = node?.ParentNode?.ParentNode?.ParentNode.Descendants("span").FirstOrDefault();
                if (timeNode != null)
                {
                    var date = timeNode.InnerText.TrimStart('(').TrimEnd(')').Split('/');
                    if (date != null && date.Count() == 3)
                    {
                        int year = 2000 + int.Parse(date[2]);
                        int month = int.Parse(date[1]);
                        int day = int.Parse(date[0]);
                        competitionDateTime = new DateTime(year: year, month: month, day: day);
                    }
                }                
            }
            catch (Exception ex)
            {
                m_logger.LogInformation(ex, $"Error parsing date for competition: {competitionUrl}");
            }
            return competitionDateTime;
        }

        private async Task<Stream> GetCompetitionStream(string competitionUrl)
        {
            var folderPath = m_options.Value.LocalStoragePath;            
            var fileName= competitionUrl.Split('/').Last();
            var fullFilePath = Path.Combine(folderPath, fileName);
            if (File.Exists(fullFilePath))
            {
                return File.OpenRead(fullFilePath);
            }
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, competitionUrl);
            httpRequestMessage.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue(@"ArcticPuzzler", "1.0.0.0"));
            var response = await m_httpClient.SendAsync(httpRequestMessage);
            if (!response.IsSuccessStatusCode)
            {
                m_logger.LogInformation($"Could not get pdf for {competitionUrl} with response code {response.StatusCode}");
                throw new Exception("Could not read pdf file");
            }
            else
            {
                var stream = await response.Content.ReadAsStreamAsync();
                using (var fileStream = File.Create(fullFilePath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    return stream;
                }

            }          
        }

        private string GetCompetitionName(Stream stream)
        {
            stream.Position = 0;
            using (var pdf = PdfDocument.Open(stream, new ParsingOptions() { ClipPaths = true }))
            {
                var rows = GetTableRows(pdf);
                var page = pdf.GetPages().FirstOrDefault();
                if(page == null)
                {
                    return string.Empty;
                }
                var text = ContentOrderTextExtractor.GetText(page);
                var textLines = text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                var firstLine = textLines.First();
                return firstLine.Replace("Results", "").TrimEnd();
            }
        }

        public async Task ParsePdf(Stream stream, DateTime competitionDate, Guid competitionId)
        {            
            var competitionGroup = new CompetitionGroupDTO();
            competitionGroup.CompetitionId = competitionId;
            var competitionRound = new CompetitionRoundDTO();
            competitionRound.Time= competitionDate;
            stream.Position = 0;
            BrandName brandName = BrandName.Unknown;
            string puzzleName = string.Empty;
            using (var pdf = PdfDocument.Open(stream, new ParsingOptions() { ClipPaths = true }))
            {                

                var rows = GetTableRows(pdf);
                foreach (var page in pdf.GetPages())
                {
                    var text = ContentOrderTextExtractor.GetText(page);
                    var textLines = text.Split(new string[] { "\r\n", "\r", "\n" },StringSplitOptions.None);
                    switch (text.ToLower())
                    {
                        case string a when a.Contains("team"):
                            competitionRound.ContestType = ContestType.Teams;
                            competitionGroup.ContestType = ContestType.Teams;                            
                            break;
                        case string b when b.Contains("pair"):
                            competitionRound.ContestType = ContestType.Pairs;
                            competitionGroup.ContestType = ContestType.Pairs;                           
                            break;
                        case string c when c.Contains("solo"):
                        case string d when d.Contains("individual"):
                            competitionRound.ContestType = ContestType.Individual;
                            competitionGroup.ContestType = ContestType.Individual;
                            break;
                        default:
                            competitionRound.ContestType = ContestType.Individual;
                            competitionGroup.ContestType = ContestType.Individual;                           
                            break;
                    }
                    competitionGroup = await m_competitionGroupApiClient.CreateOrUpdateAsync(competitionGroup);
                    competitionRound.CompetitionGroupId = competitionGroup.Id;
                    try
                    {

                        if (brandName == BrandName.Unknown)
                        {
                            string puzzleBrandString = textLines.FirstOrDefault(text => text.ToLower().Contains("piece") && !text.ToLower().Contains("proficiency")) ?? string.Empty;

                            if (!string.IsNullOrEmpty(puzzleBrandString))
                            {
                                var puzzleNameIndex = textLines.ToList().IndexOf(puzzleBrandString) + 1;
                                if (puzzleNameIndex < textLines.Count())
                                {
                                    puzzleName = textLines[puzzleNameIndex];
                                }
                                string puzzleBrand = puzzleBrandString.Split(" ")?.FirstOrDefault() ?? string.Empty;
                                puzzleBrand = WashPuzzleBrandName(puzzleBrand);
                                brandName = puzzleBrand.GetEnumFromString<BrandName>();
                            }
                        }
                        competitionRound.Puzzles.Add(new Puzzle { BrandName = brandName, Name = puzzleName });
                    }
                    catch (Exception ex)
                    {
                        m_logger.LogInformation(ex, $"Error parsing puzzle brandname/puzzle name ");
                    }

                    if (competitionRound.NumberOfPieces == 0 )
                    {
                        var numberOfPiecesString = textLines.FirstOrDefault(text => text.ToLower().Contains("piece") && !text.ToLower().Contains("proficiency"));
                        if (!string.IsNullOrEmpty(numberOfPiecesString))
                        {
                            numberOfPiecesString = Regex.Match(numberOfPiecesString, @"\d+").Value;
                            if(int.TryParse(numberOfPiecesString,out int numberOfPieces))
                            {
                                competitionRound.NumberOfPieces = numberOfPieces;
                            }
                        }
                    }                  
                    
                    if (string.IsNullOrEmpty(competitionRound.RoundName))
                    {
                        var roundLine = textLines.FirstOrDefault(t=> t.StartsWith("(") && t.EndsWith(")"));
                        if (!string.IsNullOrEmpty(roundLine))
                        {
                            roundLine = roundLine.Replace("(", string.Empty);
                            roundLine = roundLine.Replace(")", string.Empty);
                            var roundLineData = roundLine.Split("--");
                            if (roundLineData.Count() > 0)
                            {
                                competitionRound.RoundName = roundLineData[roundLineData.Count()-1];
                            }                            
                        }                        
                    }

                    competitionRound = await m_competitionRoundApiClient.CreateOrUpdateAsync(competitionRound);

                    var topRow = rows.First().ToArray();
                    var secondRow = rows[1].ToArray();
                    int timeHeader = 0;
                    int nameHeader = 0;
                    int rankHeader = 0;
                    int countryHeader = 0;
                    int piecesPerMinuteHeader = 0;
                    int skipNumber = 1;
                    if (topRow.Any(t => t.GetText().ToLower().Equals("this contest")))
                    {
                        timeHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Equals("time"));
                        nameHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("name"));
                        rankHeader = Array.FindIndex(secondRow, t => t.GetText().ToLower().Contains("rank"));
                        countryHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("location"));
                        piecesPerMinuteHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("*p4") || t.GetText().ToLower().Contains("*p6"));
                        skipNumber = 2;
                    }
                    else
                    {
                        timeHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("time") );
                        nameHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("name"));
                        rankHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("rank"));
                        countryHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("location"));
                        piecesPerMinuteHeader = Array.FindIndex(topRow, t => t.GetText().ToLower().Contains("*p4") || t.GetText().ToLower().Contains("*p6"));
                    }
                    switch (text.ToLower())
                    {
                        case string a when a.Contains("team"):
                            await AddResultForTeam(competitionRound.Id, rows, skipNumber, timeHeader, nameHeader, countryHeader, rankHeader, piecesPerMinuteHeader);
                            break;
                        case string b when b.Contains("pair"):
                            await AddResultForPairs(competitionRound.Id, rows, skipNumber, timeHeader, nameHeader, countryHeader, rankHeader, piecesPerMinuteHeader);
                            break;
                        case string c when c.Contains("solo"):
                        case string d when d.Contains("individual"):
                            await AddResultForIndividual(competitionRound.Id, rows, skipNumber, timeHeader, nameHeader, countryHeader, rankHeader, piecesPerMinuteHeader);
                            break;
                        default:
                            await AddResultForIndividual(competitionRound.Id, rows, skipNumber, timeHeader, nameHeader, countryHeader, rankHeader, piecesPerMinuteHeader);
                            break;
                    }
                      
                }
            }             
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

        private async Task AddResultForTeam(Guid competitionRoundId,IReadOnlyList<IReadOnlyList<Cell>> rows, int skipnumber, int timeHeader, int nameHeader, int countryHeader, int rankHeader, int piecesPerMinuteHeader)
        {
            var result = new ResultDTO() { CompetitionRoundId = competitionRoundId };
            var country = Countries.UNK;
            int i = 0;
            foreach (var row in rows.Skip(skipnumber))
            {
                i++;
                if (i == 3)
                {
                    result.AddResult(row, timeHeader, piecesPerMinuteHeader);
                    result.AddRank(row, rankHeader);
                    var countryString = row[countryHeader].GetText();
                    country = countryString.GetEnumFromString<Countries>();
                    if (country == Countries.UNK)
                    {
                        country = Countries.USA;
                    }
                }
                if (i != 3)
                {
                    await AddParticipant(result , row, nameHeader, countryHeader);
                }
                if(i ==5)
                {
                    foreach(var puzzleResult in result.Results)
                    {
                        puzzleResult.ParsedPiecesPerMinute = puzzleResult.ParsedPiecesPerMinute.HasValue ? puzzleResult.ParsedPiecesPerMinute.Value * result.PlayerIds.Count() : null;
                    }
                    result = await m_resultApiClient.CreateOrUpdateAsync(result);
                    result = new ResultDTO() { CompetitionRoundId = competitionRoundId };
                    country = Countries.UNK;
                    i = 0;
                    
                }            

            }

            
        }
        private async Task AddResultForPairs(Guid competitionRoundId,IReadOnlyList<IReadOnlyList<Cell>> rows, int skipnumber, int timeHeader, int nameHeader, int countryHeader, int rankHeader, int piecesPerMinuteHeader)
        {
            var result = new ResultDTO() { CompetitionRoundId = competitionRoundId };
            int i = 0;
            var country = Countries.UNK;
            foreach (var row in rows.Skip(skipnumber))
            {
                i++;
                if (!string.IsNullOrEmpty(row[rankHeader].GetText()))
                {
                    result.AddResult(row, timeHeader, piecesPerMinuteHeader);
                    result.AddRank(row, rankHeader);
                    var countryString = row[countryHeader].GetText();
                    country = countryString.GetEnumFromString<Countries>();
                    if (country == Countries.UNK)
                    {
                        country = Countries.USA;
                    }
                }
                
                if( i != 2) 
                { 
                    await AddPairParticipants(result, row, nameHeader, countryHeader);
                }
                if (i == 3 || (i == 1 && !string.IsNullOrEmpty(row[rankHeader].GetText())))
                {
                    foreach (var puzzleResult in result.Results)
                    {
                        puzzleResult.ParsedPiecesPerMinute = puzzleResult.ParsedPiecesPerMinute.HasValue ? puzzleResult.ParsedPiecesPerMinute.Value * result.PlayerIds.Count() : null;
                    }
                    result = await m_resultApiClient.CreateOrUpdateAsync(result);
                    result = new ResultDTO() { CompetitionRoundId = competitionRoundId };
                    country = Countries.UNK;
                    i = 0;                   
                }
            }
        }
        private async Task AddResultForIndividual(Guid competitionRoundId, IReadOnlyList<IReadOnlyList<Cell>> rows,int skipnumber, int timeHeader, int nameHeader, int countryHeader, int rankHeader, int piecesPerMinuteHeader)
        {
            foreach (var row in rows.Skip(skipnumber))
            {
                var result = new ResultDTO() { CompetitionRoundId = competitionRoundId };
                result.AddResult(row, timeHeader, piecesPerMinuteHeader);
                result.AddRank(row, rankHeader);

                await AddParticipant(result,row, nameHeader, countryHeader);
                result = await m_resultApiClient.CreateOrUpdateAsync(result);
            }
        }

        private static IReadOnlyList<IReadOnlyList<Cell>> GetTableRows(PdfDocument pdf)
        {
            IReadOnlyList<IReadOnlyList<Cell>> rows = new List<IReadOnlyList<Cell>>();
            for (int i = 1; i <= pdf.NumberOfPages; i++)
            {
                PageArea tablePage = ObjectExtractor.Extract(pdf,1);
                // detect canditate table zones
                SimpleNurminenDetectionAlgorithm detector = new SimpleNurminenDetectionAlgorithm();
                var regions = detector.Detect(tablePage);

                IExtractionAlgorithm ea = new BasicExtractionAlgorithm();
                var tables = ea.Extract(tablePage.GetArea(regions[0].BoundingBox)); // take first candidate area
                var table = tables[0];
                rows = table.Rows;
            }
            return rows;
        }

        public static string GetBaseUrl(string url)
        {
            var uri = new Uri(url);
            var baseUri = uri.GetLeftPart(UriPartial.Authority);
            return baseUri;
        }

        public bool SupportCompetitionType(CompetitionOwner competitionType)
        {
            return competitionType == CompetitionOwner.SpeedPuzzling;
        }

        public async Task AddParticipant(ResultDTO participant, IReadOnlyList<Cell> row, int nameHeader, int countryHeader)
        {
            var fullname = row[nameHeader].GetText();
            var countryString = row[countryHeader].GetText();
            var country = countryString.GetEnumFromString<Countries>();
            if (country == Countries.UNK)
            {
                country = Countries.USA;
            }
            var name = fullname.FixName();
            var players = await m_playerStore.GetPlayersByNameAsync(name);

            PlayerDTO? player = null;
            if (players != null && players.Count() > 0)
            {
                player = players.First();
            }
            if (player == null)
            {
                player = new PlayerDTO { FullName = fullname.FixName(), Country = country };
                await m_playerStore.CreateOrUpdatePlayerAsync(player.Id,player);
            }
            participant.PlayerIds.Add(player.Id);
        }

        public async Task AddPairParticipants(ResultDTO result, IReadOnlyList<Cell> row, int nameHeader, int countryHeader)
        {
            var fullnames = row[nameHeader].GetText().Split("\r");
            var countryString = row[countryHeader].GetText();
            var country = countryString.GetEnumFromString<Countries>();
            if (country == Countries.UNK)
            {
                country = Countries.USA;
            }
            foreach (var fullname in fullnames)
            {
                var name = fullname.FixName();
                var players = await m_playerStore.GetPlayersByNameAsync(name);

                PlayerDTO? player = null;
                if (players != null && players.Count() > 0)
                {
                    player = players.First();
                }
                if (player == null)
                {
                    player = new PlayerDTO { FullName = fullname.FixName(), Country = country };
                    await m_playerStore.CreateOrUpdatePlayerAsync(player.Id ,player);
                }
                result.PlayerIds.Add(player.Id);
            }
        }
    }
}
