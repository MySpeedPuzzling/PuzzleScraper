using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Arctic.Puzzlers.Stores;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public class WorldJigsawPuzzleOrgParser : ICompetitionParser
    {
        private readonly ILogger<WorldJigsawPuzzleOrgParser> m_logger;
        private readonly ICompetitionStore m_store;
        private readonly IPlayerStore m_playerStore;
        public WorldJigsawPuzzleOrgParser(ILogger<WorldJigsawPuzzleOrgParser> logger, ICompetitionStore store, IPlayerStore playerStore)
        {
            m_logger = logger;
            m_store = store;
            m_playerStore = playerStore;
        }
        public async Task Parse(string baseurl)
        {
            var web = new HtmlWeb();
            var mainPage = await web.LoadFromWebAsync(baseurl);
            var results = mainPage.DocumentNode.SelectNodes("//li[a='Results']").FirstOrDefault();
            if(results == null)
            {
                return;
            }
            var linksToResults = results.Descendants().Where(t => t.Name == "a");
           
            foreach( var link in linksToResults)
            {
                if(TryResolveCompetitionUrl(baseurl, link, out var competitionUrl))
                {
                    try
                    {
                        var competitionPage = await web.LoadFromWebAsync(competitionUrl);
                        if (competitionPage == null)
                        {
                            continue;
                        }

                        var competitionName = competitionPage.DocumentNode.SelectNodes("//p[contains(@class,'nombre_campeonato')]");
                        var listOfRounds = competitionPage.DocumentNode.SelectNodes("//nav[contains(@class,'nav-underline')]/a");
                        var competition = new Competition();
                        if (competitionName != null && competitionName.Count() == 1)
                        {
                            competition.Name = competitionName[0].InnerText;
                        }
                       

                        var individualRounds = listOfRounds.Where(t => t.GetAttributeValue("href", "").ToLower().Contains("individual"));
                        var pairRounds = listOfRounds.Where(t => t.GetAttributeValue("href", "").ToLower().Contains("pairs"));
                        var teamRounds = listOfRounds.Where(t => t.GetAttributeValue("href", "").ToLower().Contains("teams"));

                        competition.Url = competitionUrl;
                        if (individualRounds.Any())
                        {
                            var individualGroup = await AddRound(baseurl, individualRounds, ContestType.Individual);
                            if (individualGroup != null)
                            {
                                competition.CompetitionGroups.Add(individualGroup);
                            }
                        }

                        if (pairRounds.Any())
                        {
                            var pairsGroup = await AddRound(baseurl, pairRounds, ContestType.Pairs);
                            if (pairsGroup != null)
                            {
                                competition.CompetitionGroups.Add(pairsGroup);
                            }
                        }
                        if (teamRounds.Any())
                        {
                            var teamGroup = await AddRound(baseurl, teamRounds, ContestType.Teams);
                            if (teamGroup != null)
                            {
                                competition.CompetitionGroups.Add(teamGroup);
                            }
                        }
                        competition.SetTotalResults();
                        await m_store.Store(competition);
                    }                     
                    catch (Exception ex)
                    {
                        m_logger.LogInformation(ex, $"Could not parse data from {competitionUrl} due to exception");
                    }
            }             
            }
        }

        internal async Task AddResult(CompetitionRound competitionRound, HtmlNodeCollection values, int namefield, int timefield, int rankField)
        {
            var participantResult = new ParticipantResult();
            HtmlNodeCollection namesfield;
            namesfield = values[namefield].SelectNodes("div");
         
            
            if(namesfield.Count() < 2 )
            {
                return;
            }
            
            if (namesfield[1].InnerHtml.Contains("<div"))
            {
                var teamfields= namesfield[1].SelectNodes("div");

                participantResult.GroupName = teamfields[0].InnerText;
                var names = teamfields[1].SelectSingleNode("div").InnerHtml.Split("/");
                if (names.Any())
                {
                    for (int i = 0; names.Count() > i; i++)
                    {
                        await AddPlayer(participantResult, names[i]);
                    }
                }
            }
            else
            {
                var names = namesfield[1].InnerHtml.Split("<br>");
                if (names.Any())
                {
                    for (int i = 0; names.Count() > i; i++)
                    {
                        await AddPlayer(participantResult, names[i]);
                    }
                }
            }
            string result = string.Empty;
            bool qualified = false;
            result = values[timefield].InnerText;
            qualified= values[timefield].SelectSingleNode("//i[contains(@class,'text-success')]") != null;         
            
            
            var singlePuzzleResult = new Result();
            singlePuzzleResult.Puzzle = competitionRound.Puzzles.First();
            if (Regex.IsMatch(result, @"\d\d:\d\d:\d\d"))
            {
                singlePuzzleResult.Time = TimeSpan.Parse(Regex.Match(result, @"\d\d:\d\d:\d\d").Value);                
            }
            else
            {
                singlePuzzleResult.FinishedPieces = long.Parse(Regex.Match(result, @"\d+").Value);
            }


            if(int.TryParse(values[rankField].InnerText,out int rank))
            {
                participantResult.Rank = rank;
            }
            if (competitionRound.RoundType != RoundType.Final)
            {
                participantResult.Qualified = qualified;
            }
            participantResult.Results.Add(singlePuzzleResult);

            competitionRound.Participants.Add(participantResult);
        }

        private async Task AddPlayer(ParticipantResult participantResult, string name)
        {
            var players = await m_playerStore.FindPlayerByName(name);
            Player? player = null;
            if (players != null && players.Count() > 0)
            {
                player = players.First();
            }
            if (player == null)
            {
                player = new Player { FullName = name};
                await m_playerStore.Store(player);
            }

            participantResult.Participants.Add(new Participant { FullName = player.FullName, PlayerId = player.Id });

        }

        private async Task<CompetitionGroup> AddRound(string baseUrl, IEnumerable<HtmlNode> individualRounds, ContestType contestType)
        {
            var competitionGroup = new CompetitionGroup();
            competitionGroup.ContestType = contestType;
            foreach (var link in individualRounds)
            {
                var currentUrl = baseUrl + link.GetAttributeValue("href","");
                var competitionRound = new CompetitionRound();
                competitionRound.Url = currentUrl;

                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(currentUrl);

                competitionRound.Puzzles.Add(new Puzzle { BrandName = BrandName.Unknown, ShortId = 0 });

                competitionRound.RoundName = currentUrl.Split('/').Last().ToLower() == contestType.ToString().ToLower() ? "Final" : currentUrl.Split('/').Last();

                competitionRound.RoundType = competitionRound.RoundName.ToLower() == "final"? RoundType.Final: competitionRound.RoundName.ToLower().StartsWith('s') ? RoundType.Semifinal : RoundType.Qualification;
                var placeAndTime = doc.DocumentNode.SelectSingleNode("//p[@class='lead']").InnerText;
                var date = doc.DocumentNode.SelectNodes("//span[contains(i/@class,'bi-calendar3')]");
                var clock = doc.DocumentNode.SelectNodes("//span[contains(i/@class,'bi-clock')]");
                if (!string.IsNullOrEmpty(placeAndTime))
                {
                    var placeAndTimeList = placeAndTime.Split('.', 2);
                    if (placeAndTimeList.Length == 2)
                    {
                        var datetimeString = placeAndTimeList[0].Replace(" ", string.Empty);
                        if (DateTime.TryParseExact(datetimeString, "dd/MM/yyyy-HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime time))
                        {
                            competitionRound.Time = time;
                        }                       
                        competitionRound.Location = placeAndTimeList[1];
                    }
                    else if(placeAndTimeList.Length == 1)
                    {
                        competitionRound.Location = placeAndTimeList[0];
                    }
                }

                if (date != null && date.Count == 1 && clock != null && clock.Count == 1)
                {
                    var datetimestring = date[0].InnerText + "-" + clock[0].InnerText;
                    datetimestring = datetimestring.Replace(" ", string.Empty);
                    if (DateTime.TryParseExact(datetimestring, "dd/MM/yyyy-HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime timeFromText))
                    {
                        competitionRound.Time = timeFromText;
                    }
                }
                

                var maxTime = doc.DocumentNode.SelectNodes("//span[contains(i/@class,'bi-stopwatch')]");
                if(maxTime != null && maxTime.Count == 1 && TimeSpan.TryParse(CleanUpString(maxTime[0].InnerText), out TimeSpan maxTimeSpan)) 
                {
                    competitionRound.MaxTime= maxTimeSpan;
                }

                var pieceCount = doc.DocumentNode.SelectNodes("//span[contains(i/@class,'bi-puzzle')]");
                if (pieceCount != null && pieceCount.Count == 1 && int.TryParse(CleanUpString(pieceCount[0].InnerText), out int pieceCountOut))
                {
                    competitionRound.NumberOfPieces = pieceCountOut;
                }
                int namefield = 3;
                int timefield = 7;
                int rankField = 0;
                foreach (HtmlNode tableLine in doc.DocumentNode.SelectNodes("//tr"))
                {
                    var fields = tableLine.SelectNodes("th");
                    if(fields != null && fields.Count()> 0)
                    {
                        for(int i =0; i < fields.Count();i++)
                        {
                            if (fields[i].InnerText.Equals("name",StringComparison.InvariantCultureIgnoreCase))
                            {
                                namefield = i;
                            }
                            if (fields[i].InnerText.Equals("time", StringComparison.InvariantCultureIgnoreCase))
                            {
                                timefield = i;
                            }
                            if (fields[i].InnerText.Equals("#", StringComparison.InvariantCultureIgnoreCase))
                            {
                                rankField = i;
                            }

                        }
                    }
                    var values = tableLine.SelectNodes("td");

                    if (values == null || values.Count() == 0)
                    {
                        continue;
                    }
                    await AddResult(competitionRound, values, namefield, timefield, rankField);
                }
                competitionRound.ContestType = contestType;
                competitionGroup.Rounds.Add(competitionRound);
            }
            return competitionGroup;

        }
        public bool SupportCompetitionType(CompetitionOwner competitionType)
        {
            return competitionType == CompetitionOwner.WJPC;
        }

        public bool TryResolveCompetitionUrl(string baseUrl, HtmlNode node, out string currentUrl)
        {
            currentUrl = baseUrl;
            var resultLink = node.GetAttributeValue("href", "");
            if (!resultLink.StartsWith('/'))
            {
                return false;
            }
            currentUrl += node.GetAttributeValue("href", "");
            return true;
        }

        private string CleanUpString(string value)
        {
            value = value.Replace("&nbsp;", string.Empty);
            value = value.Replace(" ", string.Empty);
            value = value.Replace(".", string.Empty);
            return value;
        }
    }
}
