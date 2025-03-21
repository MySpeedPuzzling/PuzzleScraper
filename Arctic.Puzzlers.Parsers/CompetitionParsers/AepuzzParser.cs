using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.DTO;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Arctic.Puzzlers.Parsers.CompetitionParsers;
using Arctic.Puzzlers.Parsers.CompetitionParsers.Options;
using Arctic.Puzzlers.Stores;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Arctic.Puzzlers.CLI.InputParsing
{
    public class AepuzzParser: ICompetitionParser
    {
        private readonly ILogger<AepuzzParser> m_logger;
        private readonly ICompetitionStore m_store;
        private readonly IPlayerStore m_playerStore;
        private readonly IOptions<CompetitionParsingOptions> m_options;

        public AepuzzParser(ILogger<AepuzzParser> logger, ICompetitionStore store, IPlayerStore playerstore, IOptions<CompetitionParsingOptions> options) 
        {
            m_logger = logger;
            m_store = store;
            m_playerStore = playerstore;
            m_options = options;
        }

        internal async Task AddResult(CompetitionRound competitor, HtmlNodeCollection values, string baseurl)
        {
            var participantResult = new ParticipantResult();
            var names = values[2].SelectNodes("div/a");
            if(names.Count() > 0) 
            {
                for(int i = 0 ; names.Count > i; i++)
                {
                    var personRelativeUrl = names[i].Attributes["href"];
                    var fullurl = baseurl + "/" +personRelativeUrl.Value;
                    var identifier = Regex.Match(personRelativeUrl.Value, @"\d+").Value;
                    var parsedIdentifier = new ParsedIdentifier { UserOwner = CompetitionOwner.AePuzz, Identifier = identifier, OriginalUrl = fullurl };
                    var name = names[i].InnerText;
                    var players = await m_playerStore.GetPlayersByNameAsync(name);
                    PlayerDTO? player = null;
                    if(players != null && players.Count() > 0)
                    {
                        player = players.First();
                    }
                    if (player == null)
                    {
                        player = new PlayerDTO { FullName = name, Country = Objects.Misc.Countries.ESP, ParsedIdentifier = new List<ParsedIdentifier>() { parsedIdentifier } };
                        await m_playerStore.CreateOrUpdatePlayerAsync(player.Id,player);
                    }

                    participantResult.Participants.Add(new Participant { FullName = player.FullName, PlayerId = player.Id });                
                }
            }
            string result;
            if (values.Count() == 10)
            {
                result = values[5].InnerText;
            }
            else
            {
                result = values[4].InnerText;
            }
            var singlePuzzleResult = new Result();
            singlePuzzleResult.Puzzle = competitor.Puzzles.First();
            if (Regex.IsMatch(result, @"\d\d:\d\d:\d\d"))
            {
                singlePuzzleResult.Time = TimeSpan.Parse(Regex.Match(result, @"\d\d:\d\d:\d\d").Value);
            }
            else
            {
                singlePuzzleResult.FinishedPieces = long.Parse(Regex.Match(result, @"\d+").Value);
            }
            participantResult.Results.Add(singlePuzzleResult);

            competitor.Participants.Add(participantResult);
        }

        public async Task Parse(string url)
        {
            string currentUrl = url;
            for (int i = 1; i < 300; i++)
            {
                var competition = new Competition();
                
                try
                {                    
                    currentUrl = url + $"/clasificacion.php?id={i}";
                    competition.Url = currentUrl;
                    var needToParse = await m_store.NeedToParse(currentUrl);
                    if (!needToParse)
                    {
                        continue;
                    }
                    var web = new HtmlWeb();
                    var mainPage = await GetCompetitionHtml(currentUrl);
                    var htmlNodes = mainPage.DocumentNode.SelectNodes("//nav[@aria-label='Secondary navigation']/a");
                    
                    var competitionSingleRound = await ParseWebPage(mainPage, currentUrl, url);                       
                    if (competitionSingleRound == null)
                    {
                        continue;
                    }
                        
                    competition.Name = competitionSingleRound.RoundName;
                    competition.Time = competitionSingleRound.Time;
                    competition.Location = competitionSingleRound.Location;
                    competitionSingleRound.RoundType = RoundType.Final;
                    if (htmlNodes == null || !htmlNodes.Any())
                    {
                        var competitionGroup = new CompetitionGroup { ContestType = competitionSingleRound.ContestType };
                        competitionGroup.Rounds.Add(competitionSingleRound);
                        competition.CompetitionGroups.Add(competitionGroup);
                        competition.SetTotalResults();
                        var addedSingleCompetition = await m_store.Store(competition);
                        if (addedSingleCompetition)
                        {
                            m_logger.LogInformation(competition.Name);
                        }
                        continue;
                    }             
               
                    foreach (HtmlNode pages in htmlNodes)
                    {
                        
                        currentUrl = url + "/" + pages.Attributes["href"].Value;                        
                        
                        var doc = await GetCompetitionHtml(currentUrl);
                        

                        var competitionObject = await ParseWebPage(doc, currentUrl, url);
                        if (competitionObject == null)
                        {
                            continue;
                        }
                        competitionObject.RoundType = RoundType.Final;
                        var competitionGroup = new CompetitionGroup { ContestType = competitionObject.ContestType };
                        competitionGroup.Rounds.Add(competitionSingleRound);
                        competition.CompetitionGroups.Add(competitionGroup);                       
                    }
                    competition.SetTotalResults();
                    var added = await m_store.Store(competition);
                    if (added)
                    {
                        m_logger.LogInformation(competition.Name);
                    }
                }
                catch (Exception ex)
                {
                    m_logger.LogInformation(ex,$"Could not parse data from {currentUrl} due to exception");
                }
            }
        }

        private async Task<HtmlDocument> GetCompetitionHtml(string competitionUrl)
        {
            var folderPath = m_options.Value.LocalStoragePath;
            var fileName = "aepuzz-" + String.Join("-",competitionUrl.Split("?").Last().Split("&").Select(t=> t.Split("=").Last())) + ".html";

            var fullFilePath = Path.Combine(folderPath, fileName);
            var web = new HtmlWeb();
            if (File.Exists(fullFilePath))
            {
                using (var filestream = File.OpenRead(fullFilePath))
                {
                    var competitionPage = new HtmlDocument();
                    competitionPage.Load(filestream);
                    return competitionPage;
                }
            }
            else
            {
                var competitionPage = await web.LoadFromWebAsync(competitionUrl);
                File.WriteAllText(fullFilePath, competitionPage.DocumentNode.OuterHtml);
                return competitionPage;
            }
        }

        public async Task<CompetitionRound?> ParseWebPage(HtmlDocument doc, string currentUrl, string baseUrl)
        {
            var competitionObject = new CompetitionRound();
            competitionObject.Url = currentUrl;
            if (doc?.DocumentNode?.InnerText?.Contains("No existe") ?? false)
            {
                return null;
            }
            var imageUrl = doc?.DocumentNode?.SelectSingleNode("//img[contains(@src,'imagenes')]")?.GetAttributeValue("src", "");
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imagename = imageUrl.Split('/').Last().Split('_');
                var brandName = string.Concat(imagename.First()[0].ToString().ToUpper(), imagename.First().AsSpan(1));
                var shortId = long.Parse(imagename.Last().Replace(".jpg", ""));
                competitionObject.Puzzles.Add(new Puzzle { BrandName = ParseBrandName(brandName), ShortId = shortId });
            }
            else
            {
                competitionObject.Puzzles.Add(new Puzzle { BrandName = BrandName.Unknown, ShortId = 0 });
            }
            competitionObject.RoundName = doc?.DocumentNode?.SelectSingleNode("//h1[@class='display-4']")?.InnerText ?? string.Empty;
            var placeAndTime = doc?.DocumentNode?.SelectSingleNode("//p[@class='lead']")?.InnerText ?? string.Empty;

            if (!string.IsNullOrEmpty(placeAndTime))
            {
                var placeAndTimeList = placeAndTime.Split('.', 2);
                if (placeAndTimeList.Length == 2)
                {
                    var datetimeString = placeAndTimeList[0].Replace(" ", string.Empty);
                    if (DateTime.TryParseExact(datetimeString, "dd/MM/yyyy-HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime time))
                    {
                        competitionObject.Time = time;
                    }
                    competitionObject.Location = placeAndTimeList[1];
                }
            }

            foreach (HtmlNode tableLine in doc?.DocumentNode?.SelectNodes("//tr"))
            {
                var values = tableLine.SelectNodes("td");
                if (values == null || values.Count() == 0)
                {
                    continue;
                }
                await AddResult(competitionObject, values, baseUrl);
            }
            competitionObject.SetContestType();
            return competitionObject;
        } 
        
        public BrandName ParseBrandName(string name)
        {
            if(name == null)
            {
                return BrandName.Unknown;
            }
            if(name.Equals("RAVENSBURGER", StringComparison.InvariantCultureIgnoreCase))
            {
                return BrandName.Ravensburger;
            }
            if (name.Equals("SCHMIDT", StringComparison.InvariantCultureIgnoreCase))
            {
                return BrandName.Schmidt;
            }
            if (name.Equals("CLEMENTONI", StringComparison.InvariantCultureIgnoreCase))
            {
                return BrandName.Clementoni;
            }
            if (name.Equals("EDUCA", StringComparison.InvariantCultureIgnoreCase))
            {
                return BrandName.Educa;
            }
            return BrandName.Unknown;
        }

        public bool SupportCompetitionType(CompetitionOwner competitionType)
        {
            return competitionType.Equals(CompetitionOwner.AePuzz);
        }
    }
}
