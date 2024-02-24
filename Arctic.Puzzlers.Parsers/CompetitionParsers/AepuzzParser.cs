using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Objects.PuzzleObjects;
using Arctic.Puzzlers.Parsers.CompetitionParsers;
using Arctic.Puzzlers.Stores;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Arctic.Puzzlers.CLI.InputParsing
{
    public class AepuzzParser: ICompetitionParser
    {
        private readonly ILogger<AepuzzParser> m_logger;
        private readonly ICompetitionStore m_store;

        public AepuzzParser(ILogger<AepuzzParser> logger, ICompetitionStore store) 
        {
            m_logger = logger;
            m_store = store;
        }

        internal void ResolveContestType(Competition competitionObject)
        {
            if(competitionObject.Participants.All(t=> t.Participants.Count <=1))
            {
                competitionObject.ContestType = ContestType.Individual;
            }
            else if (competitionObject.Participants.All(t => t.Participants.Count <=2))
            {
                competitionObject.ContestType = ContestType.Pair;
            }
            else if (competitionObject.Participants.All(t => t.Participants.Count <= 4))
            {
                competitionObject.ContestType = ContestType.Team;
            }
            else
            {
                competitionObject.ContestType = ContestType.Unknown;
            }
        }


        internal void AddResult(Competition competitor, HtmlNodeCollection values)
        {
            var participantResult = new ParticipantResult();
            var names = values[2].SelectNodes("div/a");
            if(names.Count() > 0) 
            {
                for(int i =0 ; names.Count > i; i++)
                {
                    participantResult.Participants.Add(new Participant { FullName= names[i].InnerText, Country= Objects.Misc.Countries.ESP });                
                }
            }
            var result = values[4].InnerText;
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
            for (int i = 1; i < 500; i++)
            {
                string currentUrl = url;                
                try
                {
                    for (int x = 1; x < 4; x++)
                    {
                        var competitionObject = new Competition();
                        currentUrl = url + $"?id={i}&cat={x}";
                        var needToParse = await m_store.NeedToParse(currentUrl);
                        if (!needToParse)
                        {
                            continue;
                        }
                        competitionObject.Url = currentUrl;
                        var web = new HtmlWeb();
                        var doc = web.Load(currentUrl);

                        if(doc?.DocumentNode?.InnerText?.Contains("No existe") ?? false)
                        {
                            continue;
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
                        competitionObject.Name = doc.DocumentNode.SelectSingleNode("//h1[@class='display-4']").InnerText;
                        var placeAndTime = doc.DocumentNode.SelectSingleNode("//p[@class='lead']").InnerText;

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

                        foreach (HtmlNode tableLine in doc.DocumentNode.SelectNodes("//tr"))
                        {
                            var values = tableLine.SelectNodes("td");
                            if (values == null || values.Count() == 0)
                            {
                                continue;
                            }
                            AddResult(competitionObject, values);
                        }
                        ResolveContestType(competitionObject);                       
                        
                        
                        var added = await m_store.Store(competitionObject);
                        if(added)
                        {
                            m_logger.LogInformation(competitionObject.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_logger.LogInformation(ex,$"Could not parse data from {currentUrl} due to exception");
                }
            }
        }
        
        public BrandName ParseBrandName(string name)
        {
            if(name == null)
            {
                return BrandName.Unknown;
            }
            if(name.ToUpperInvariant()== "RAVENSBURGER")
            {
                return BrandName.Ravensburger;
            }
            if (name.ToUpperInvariant() == "SCHMIDT")
            {
                return BrandName.Schmidt;
            }
            if (name.ToUpperInvariant() == "CLEMENTONI")
            {
                return BrandName.Clementoni;
            }
            if (name.ToUpperInvariant() == "EDUCA")
            {
                return BrandName.Educa;
            }
            return BrandName.Unknown;
        }

        public bool SupportCompetitionType(CompetitionType competitionType)
        {
            return competitionType.Equals(CompetitionType.AePuzz);
        }
    }
}
