
using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.DTO;
using Arctic.Puzzlers.Objects.Misc;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Arctic.Puzzlers.Parsers.CompetitionParsers.Options;
using Arctic.Puzzlers.Stores;
using Arctic.Puzzlers.Stores.APIStores;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public class SpeedPuzzleEuParser : ICompetitionParser
    {
        private readonly ILogger<SpeedPuzzleEuParser> m_logger;
        private readonly IPlayerStore m_playerStore;
        private readonly IOptions<CompetitionParsingOptions> m_options;
        private readonly IResultApiClient m_resultApiClient;
        private readonly ICompetitionApiClient m_competitionApiClient;
        private readonly ICompetitionGroupApiClient m_competitionGroupApiClient;
        private readonly ICompetitionRoundApiClient m_competitionRoundApiClient;    

        public SpeedPuzzleEuParser(ICompetitionApiClient competitionApiClient,ICompetitionGroupApiClient competitionGroupApiClient,ICompetitionRoundApiClient competitionRoundApiClient, IResultApiClient resultApiClient, ILogger<SpeedPuzzleEuParser> logger, IPlayerStore playerStore, IOptions<CompetitionParsingOptions> options)
        {
            m_logger = logger;
            m_playerStore = playerStore;
            m_options = options;
            m_competitionApiClient = competitionApiClient;
            m_competitionGroupApiClient = competitionGroupApiClient;
            m_competitionRoundApiClient = competitionRoundApiClient;
            m_resultApiClient = resultApiClient;
        }
        public async Task Parse(string url)
        {
            var web = new HtmlWeb();
            var mainPage = await web.LoadFromWebAsync(url);
            var googleSheets = mainPage.DocumentNode.SelectNodes("//iframe");
            if (!googleSheets.Any())
            {
                m_logger.LogInformation($"Could not find any googlesheets {url}");
                return;
            }
            string currentUrl = string.Empty;
            foreach(var sheet in googleSheets)
            {
                try
                {
                    currentUrl = sheet.GetAttributeValue("src", string.Empty);

                    var currentGoogleSheet = await web.LoadFromWebAsync(currentUrl);

                    var competitionData = currentGoogleSheet.DocumentNode.SelectNodes("//table");

                    foreach(var currentTable in competitionData)
                    {
                        try
                        {
                            await ParseTable(currentTable);
                            
                        }
                        catch (Exception e)
                        {
                            m_logger.LogError(e, $"Failed parsing {currentUrl}");
                        }
                    }
                    
                }
                catch (Exception e) 
                {
                    m_logger.LogError(e,$"Failed parsing {currentUrl}");
                }
            }
        }

        private async Task ParseTable(HtmlNode competitionData)
        {
           
            var tableBody = competitionData.Descendants("tbody").FirstOrDefault();
            if(tableBody != null)
            {
                var tableData = tableBody.Descendants("tr")?.ToArray();
                if (tableData != null && tableData.Length > 0)
                {
                    var uniqueName = ParseCompetitionName(tableData);
                    var competition = await m_competitionApiClient.GetCompetitionByNameAsync(uniqueName) ?? new CompetitionDTO();

                    competition.Name = ParseCompetitionName(tableData);
                    competition.UniqueName = ParseCompetitionName(tableData);
                    competition.Location = ParseLocation(tableData);
                    competition.Time = ParseDate(tableData);
                    competition = await m_competitionApiClient.CreateOrUpdateAsync(competition);
                    var competitionGroup = new CompetitionGroupDTO() { CompetitionId = competition.Id};
                    
                   
                    competitionGroup.ContestType = ParseContestType(tableBody);
                    competitionGroup = await m_competitionGroupApiClient.CreateOrUpdateAsync(competitionGroup);

                    var competitionRound = new CompetitionRoundDTO() { CompetitionGroupId = competitionGroup.Id };
                    competitionRound.Puzzles.Add(ParsePuzzle(tableData));
                    competitionRound.Location = competition.Location;
                    competitionRound.RoundName = "Finale";
                    competitionRound.Time = competition.Time;
                    competitionRound.ContestType = competitionGroup.ContestType;
                    competitionRound.NumberOfPieces = competitionRound.Puzzles.Select(t => t.Size).Sum();
                    competitionRound = await m_competitionRoundApiClient.CreateOrUpdateAsync(competitionRound);
                    foreach (var tr in tableData)
                    {
                        var dataFields = tr.Descendants("td").ToArray();
                        if (dataFields.First().InnerHtml.EndsWith('.'))
                        {
                            var participants = await ParseParticipants(dataFields, competitionRound.ContestType);
                            participants.CompetitionRoundId = competitionRound.Id;
                            participants.Results.ForEach(t => t.Puzzle = competitionRound.Puzzles.FirstOrDefault() ?? new Puzzle());

                            await m_resultApiClient.CreateOrUpdateAsync(participants);
                        }
                    }
                }
            }            
        }

        private DateTime ParseDate(HtmlNode[] tableBody)
        {
            var date = string.Concat(tableBody[6].Descendants("td").Select(t => t.InnerText)).Split('.');
            if (date != null && date.Count() == 3)
            {
                int year = int.Parse(date[2]);
                int month = int.Parse(date[1]);
                int day = int.Parse(date[0]);
                return new DateTime(year: year, month: month, day: day);
            }
            return new DateTime() ;
        }

        private string ParseCompetitionName(HtmlNode[] tableBody)
        {
            return string.Concat(tableBody[5].Descendants("td").Select(t => t.InnerText));
        }

        private string ParseLocation(HtmlNode[] tableBody)
        {
            var isVirtual= string.Concat(tableBody[5].Descendants("td").Select(t => t.InnerText)).Contains("#");
            if (isVirtual)
            {
                return "Virtual";
            }
            else
            {
                return string.Concat(tableBody[5].Descendants("td").Select(t => t.InnerText)).Split(" ").Last();
            }
        }

        private async Task<ResultDTO> ParseParticipants(HtmlNode[] dataFields, ContestType type)
        {
            var result = new ResultDTO();
            if(int.TryParse(dataFields[0].InnerHtml.Replace(".", ""), out int rank))
            {
                result.Rank = rank;
            }
            int modifier = 1;
            switch (type)
            {
                case ContestType.Pairs:
                    modifier = 2;
                    break;
            }
            for(int i= 1; i<=modifier; i++)
            {
                var name= dataFields[i].InnerHtml.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    if (type == ContestType.Teams)
                    {
                        result.GroupName = name;
                    }
                    else
                    {
                        var players = await m_playerStore.GetPlayersByNameAsync(name);
                        PlayerDTO? player = null;
                        if (players != null && players.Count() > 0)
                        {
                            player = players.First();
                        }
                        if (player == null)
                        {
                            player = new PlayerDTO { FullName = name };
                            await m_playerStore.CreateOrUpdatePlayerAsync(player.Id, player);
                        }

                        result.PlayerIds.Add(player.Id);
                    }
                }     
            }
            var timeField = dataFields[modifier + 2].InnerHtml;
            var resultData = new PuzzleResult();
            if (Regex.IsMatch(timeField, @"\d:\d\d:\d\d"))
            {
                resultData.PuzzleTime = TimeSpan.Parse(Regex.Match(timeField, @"\d:\d\d:\d\d").Value);
            }
            else
            {
                resultData.FinishedPieces = long.Parse(Regex.Match(timeField, @"\d+").Value);
            }

            var parsedPPMField = dataFields[modifier + 3].InnerHtml;
            if (double.TryParse(parsedPPMField, out double piecesPerMinute))
            {
                resultData.ParsedPiecesPerMinute = piecesPerMinute;
            }

            result.Results.Add(resultData);
            result.FinishedPieces = resultData.FinishedPieces;
            result.TotalTime = resultData.PuzzleTime;
            return result;
        }

        private Puzzle ParsePuzzle(HtmlNode[] tableData)
        {
            try
            {
                var parsedBrandName = string.Concat(tableData[1].Descendants("td").Select(t => t.InnerText));
                Puzzle puzzle = new Puzzle();
                if (!string.IsNullOrEmpty(parsedBrandName))
                {
                    var puzzleBrand = parsedBrandName.GetEnumFromString<BrandName>();
                    puzzle.BrandName = puzzleBrand;
                }
                puzzle.Name = string.Concat(tableData[2].Descendants("td").Select(t => t.InnerText));
                var puzzleSize = string.Concat(tableData[3].Descendants("td").Select(t => t.InnerText));
                if (!string.IsNullOrEmpty(puzzleSize) && int.TryParse(string.Concat(puzzleSize.Where(Char.IsDigit)), out int outputsize))
                {
                    puzzle.Size = outputsize;
                }
                else
                {
                    puzzle.Size = 500;
                }
                return puzzle;
            }
            catch (Exception e)
            {
                m_logger.LogError(e,"Error trying to parse puzzle");
                return new Puzzle { Size = 500 };
            }
        }

        private static ContestType ParseContestType(HtmlNode tableBody)
        {
            if( tableBody.Descendants("td").Any(t => t.InnerText.Contains("pairs", StringComparison.InvariantCultureIgnoreCase)))
            {
                return ContestType.Pairs;
            }
            if(tableBody.Descendants("td").Any(t => t.InnerText.Contains("teams", StringComparison.InvariantCultureIgnoreCase)))
            {
                return ContestType.Teams;
            }
            return ContestType.Individual;          
        }

        public bool SupportCompetitionType(CompetitionOwner competitionType)
        {
            return CompetitionOwner.SpeedPuzzleEu == competitionType;
        }
    }
}
