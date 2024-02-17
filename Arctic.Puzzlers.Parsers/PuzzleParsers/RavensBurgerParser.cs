using Arctic.Puzzlers.Objects.PuzzleObjects;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Arctic.Puzzlers.Parsers.PuzzleParsers
{
    public class RavensBurgerParser : IPuzzlePageParser
    {
        private ILogger<RavensBurgerParser> m_logger;

        private List<long> m_allowedSizes = new List<long>() { 200, 300, 500, 1000 };

        public RavensBurgerParser(ILogger<RavensBurgerParser> logger) 
        {
            m_logger = logger;
        }
        public async Task<List<PuzzleExtended>> Parse(string url)
        {
            var linksToPuzzles = GetAllLinksPerPage(url);
            var puzzles = new List<PuzzleExtended>();
            foreach (var link in linksToPuzzles)
            {
                var puzzleUrl = GetRavensburgerBaseUrl(url) + link;
                try
                {
                    var puzzle = await ParseSpecificPage(puzzleUrl);  
                    if(puzzle != null)
                    {
                        puzzles.Add(puzzle);
                        m_logger.LogInformation(puzzle.ToString());
                    }                   
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    m_logger.LogError(e, "Could not parse puzzle from ravensburger. Waiting 15 sec before taking next");
                    Thread.Sleep(15000);
                    continue;
                }

            }
            return puzzles;
        }

        private async Task<PuzzleExtended?> ParseSpecificPage(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var numberOfPiecesNode = doc.DocumentNode.SelectSingleNode("//div[starts-with(@title,'Piece Count')]/a/div[not(@class)]");
            var barcode = doc.DocumentNode.SelectSingleNode("//div[@class='specificationContainer']");
            var title = doc.DocumentNode.SelectSingleNode("//title");
            var puzzleObject = new PuzzleExtended();
            if(numberOfPiecesNode == null)
            {
                return null;
            }
            var numberOfPieces = long.Parse(string.Join("",numberOfPiecesNode.InnerText.Where(c => Char.IsDigit(c) || c == '.')));
            if(!m_allowedSizes.Any(t=> t == numberOfPieces))
            {
                return null;
            }
            puzzleObject.NumberOfPieces = numberOfPieces;
            puzzleObject.BrandName = BrandName.Ravensburger;
            puzzleObject.Identifier = string.Join("", barcode.InnerText.Where(c => Char.IsDigit(c) || c == '.'));
            puzzleObject.ShortId = long.Parse(puzzleObject.Identifier.Substring(7, 5));
            var baseUrl = new Uri(url);
            var imageUrl = baseUrl?.Scheme + "://" + baseUrl?.Authority + $"/produktseiten/500/{puzzleObject.ShortId}.webp";
            puzzleObject.Name = title.InnerText.Split('|').First().CleanUpName();
            puzzleObject.ImageUrl = imageUrl;
            return puzzleObject;
        }

        internal static string GetRavensburgerBaseUrl(string url)
        {
            var baseUrl = new Uri(url);
            var puzzleUrl = baseUrl?.Scheme + "://" + baseUrl?.Authority;
            return puzzleUrl;
        }

        internal static string GetCulturePaths(string url)
        {
            var baseUrl = new Uri(url);
            var cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures).ToList().Select(t => t.Name);
            var countryCode = baseUrl.Segments[1].Replace("/", "").ToLower();
            if (cinfo.Any(t => t.ToLower() == countryCode) || cinfo.Any(t => t.ToLower().EndsWith(countryCode)))
            {
                return "/" + baseUrl.Segments[1] + baseUrl.Segments[2];
            }
            return "/products/";
        }

        internal static async Task<byte[]> GetImage(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetByteArrayAsync(new Uri(url));
            }
        }
        internal static List<string> GetAllLinksPerPage(string url)
        {
            var linksToPuzzles = new List<string>();
            var culturePath = GetCulturePaths(url);
            for (int i = 1; i < 100; i++)
            {
                var web = new HtmlWeb();
                var doc = web.Load(url + "?page=" + i);
                var referenceToJigsaws = doc.DocumentNode.SelectNodes($"//div[@class='table-cell']/a[starts-with(@href,'{culturePath}')]");
                if (referenceToJigsaws != null && referenceToJigsaws.Count > 0)
                {
                    foreach (var reference in referenceToJigsaws)
                    {
                        var puzzlePageLink = reference.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(puzzlePageLink))
                        {
                            linksToPuzzles.Add(puzzlePageLink);
                        }
                    }
                }
                else

                {
                    break;
                }
            }
            return linksToPuzzles;

        }

        public bool SupportsBrand(BrandName brandName)
        {
            return brandName.Equals(BrandName.Ravensburger);
        }
    }
}
