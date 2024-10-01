using Arctic.Puzzlers.CLI.InputParsing;
using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.Misc;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Arctic.Puzzlers.Stores;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Arctic.Puzzlers.Parsers.CompetitionParsers
{
    public class WorldJigsawPuzzleOrgParser : ICompetitionParser
    {
        private readonly ILogger<WorldJigsawPuzzleOrgParser> m_logger;
        private readonly ICompetitionStore m_store;
        public WorldJigsawPuzzleOrgParser(ILogger<WorldJigsawPuzzleOrgParser> logger, ICompetitionStore store)
        {
            m_logger = logger;
            m_store = store;
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
                        var listOfRounds = competitionPage.DocumentNode.SelectNodes("//nav[contains(@class,'nav-underline')]/a");

                        var individualRounds = listOfRounds.Where(t => t.GetAttributeValue("href", "").ToLower().Contains("individual"));
                        var pairRounds = listOfRounds.Where(t => t.GetAttributeValue("href", "").ToLower().Contains("pairs"));
                        var teamRounds = listOfRounds.Where(t => t.GetAttributeValue("href", "").ToLower().Contains("teams"));
                        var competition = new Competition();
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
                        await m_store.Store(competition);
                    }                     
                    catch (Exception ex)
                    {
                        m_logger.LogInformation(ex, $"Could not parse data from {competitionUrl} due to exception");
                    }
            }             
            }
        }

        internal void AddResult(CompetitionRound competitionRound, HtmlNodeCollection values)
        {
            var participantResult = new ParticipantResult();
            var namesfield = values[3].SelectNodes("div");
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
                        participantResult.Participants.Add(new Participant { FullName = names[i]});
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
                        participantResult.Participants.Add(new Participant { FullName = names[i]});
                    }
                }
            }
            string result = string.Empty;
            bool qualified = false;
            if (values.Count() == 10)
            {
                result = values[7].InnerText;
                qualified= values[7].SelectSingleNode("//i[contains(@class,'text-success')]") != null;
            }
            else
            {
                result = values[6].InnerText;
                qualified = values[6].SelectSingleNode("//i[contains(@class,'text-success')]") != null;
            }
            
            
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
            
            if(competitionRound.RoundType != RoundType.Final)
            {
                participantResult.Qualified = qualified;
            }
            participantResult.Results.Add(singlePuzzleResult);

            competitionRound.Participants.Add(participantResult);
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
                }

                foreach (HtmlNode tableLine in doc.DocumentNode.SelectNodes("//tr"))
                {
                    var values = tableLine.SelectNodes("td");
                    if (values == null || values.Count() == 0)
                    {
                        continue;
                    }
                    AddResult(competitionRound, values);
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
    }
}
