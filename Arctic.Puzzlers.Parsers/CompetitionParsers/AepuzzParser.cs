using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Parsers.CompetitionParsers;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Arctic.Puzzlers.CLI.InputParsing
{
    public class AepuzzParser: ICompetitionParser
    {
        private readonly ILogger<AepuzzParser> m_logger;

        public AepuzzParser(ILogger<AepuzzParser> logger) 
        {
            m_logger = logger;
        }

        internal void ResolveContestType(Competition competitionObject)
        {
            if(competitionObject.Competitors.All(t=> string.IsNullOrEmpty(t.Person2)))
            {
                competitionObject.ContestType = "Individual";
            }
            else if (competitionObject.Competitors.All(t => string.IsNullOrEmpty(t.Person3)))
            {
                competitionObject.ContestType = "Pair";
            }
            else
            {
                competitionObject.ContestType = "Team";
            }
        }

        internal void ParseTime(P competitor, HtmlNodeCollection values)
        {
            var result = values[4].InnerText;
            if (Regex.IsMatch(result, @"\d\d:\d\d:\d\d"))
            {
                competitor.FinalTime = Regex.Match(result, @"\d\d:\d\d:\d\d").Value;
            }
            else 
            { 
                competitor.NumberOfPieces = long.Parse(Regex.Match(result, @"\d+").Value); 
            }
           
        }

        internal void ParseLocation(Competition competitor, HtmlNodeCollection values)
        {
            var location = values[3].InnerText;

            competitor.Location = location;
        }
        internal void ParseNames(Competition competitor, HtmlNodeCollection values)
        {
           
            var names = values[2].SelectNodes("div/a");
            if(names.Count() > 0) 
            {
                for(int i =0 ; names.Count > i; i++)
                {
                    switch(i)
                    {
                        case 0:
                            competitor.Person1 = names[i].InnerText;
                            break;
                        case 1:
                            competitor.Person2 = names[i].InnerText;
                            break;
                        case 2:
                            competitor.Person3 = names[i].InnerText;
                            break;
                        case 3:
                            competitor.Person4 = names[i].InnerText;
                            break;
                    }
                }
            }
        }

        public async Task<List<Competition>> Parse(string url)
        {
            var competitions = new List<Competition>();
            for (int i = 1; i < 1000; i++)
            {
                for (int x = 1; x < 4; x++)
                {
                    var currentUrl = url + $"?id={i}&cat={x}";

                    var web = new HtmlWeb();
                    var doc = web.Load(currentUrl);
                    var imageUrl = doc?.DocumentNode?.SelectSingleNode("//img[contains(@src,'imagenes')]")?.GetAttributeValue("src", "");
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        continue;
                    }
                    var imagename = imageUrl.Split('/').Last().Split('_');
                    var brandName = string.Concat(imagename.First()[0].ToString().ToUpper(), imagename.First().AsSpan(1));
                    var shortId = imagename.Last().Replace(".jpg", "");                   
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
                                competitionObject.Time = Timestamp.FromDateTime(time.ToUniversalTime());
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
                        var competitor = new Competition();

                        ParseNames(competitor,values);
                        ParseLocation(competitor,values);
                        ParseTime(competitor,values);
                        competitor.CountryCode = "es";

                        competitionObject.Competitors.Add(competitor);
                    }
                    ResolveContestType(competitionObject);
                    m_logger.LogInformation(competitionObject.Id.ToString());
                }
            }
            return competitions;
        }

        public bool SupportCompetitionType(CompetitionType competitionType)
        {
            return competitionType.Equals(CompetitionType.AePuzz);
        }
    }
}
