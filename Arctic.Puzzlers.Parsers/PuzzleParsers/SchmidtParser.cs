using Arctic.Puzzlers.Objects.PuzzleObjects;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
namespace Arctic.Puzzlers.Parsers.PuzzleParsers
{
    public class SchmidtParser : IPuzzlePageParser
    {
        private readonly ILogger<SchmidtParser> m_logger;
        private List<long> m_allowedSizes = new List<long>() { 200, 300, 500, 1000 };
        public SchmidtParser(ILogger<SchmidtParser> logger)
        {
            m_logger = logger;
        }
        public async Task<List<PuzzleExtended>> Parse(string url)
        {
            var linksToPuzzles = GetAllLinksPerPage(url);
            var puzzles = new List<PuzzleExtended>();
            foreach (var link in linksToPuzzles)
            {
                var puzzleUrl = GetBaseUrl(url) + link;
                try
                {
                    var puzzle = await ParseSpecificPage(puzzleUrl);
                    puzzles.Add(puzzle);
                    m_logger.LogInformation(puzzle.ToString());
                }
                catch (Exception e)
                {
                    m_logger.LogError(e, "Failed parsing schmidts puzzle");
                    Thread.Sleep(15000);
                    continue;
                }

            }
            return puzzles;
        }
        internal string GetBaseUrl(string url)
        {
            var baseUrl = new Uri(url);
            var puzzleUrl = baseUrl?.Scheme + "://" + baseUrl?.Authority;
            return puzzleUrl;
        }

        private async Task<PuzzleExtended> ParseSpecificPage(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var title = doc.DocumentNode.SelectSingleNode("//title");
            var titleParts = title.InnerText.Split(',');            
            var lastTitleParts = titleParts.Last().Split('-');
            var pieces = doc.DocumentNode.SelectSingleNode("//span[@class='pa']");
            string name;
            string shortid;
            if (titleParts.Length == 1 && lastTitleParts.Length == 3)
            {
                name = lastTitleParts[0];
                shortid = string.Join("", lastTitleParts[1].ToArray().Where(c => char.IsDigit(c) || c == '.'));
            }
            else if (titleParts.Length == 3)
            {
                name = titleParts[0] + titleParts[1];
                shortid = string.Join("", lastTitleParts[1].ToArray().Where(c => char.IsDigit(c) || c == '.'));
            }
            else
            {
                shortid = string.Join("", lastTitleParts[1].ToArray().Where(c => char.IsDigit(c) || c == '.'));
                name = titleParts[0];
            }
            var puzzle = new PuzzleExtended();
            var numberOfPieces = long.Parse(string.Join("", pieces.InnerText.Where(c => Char.IsDigit(c) || c == '.')));
            if(!m_allowedSizes.Any(t=> t == numberOfPieces))
            {
                return null;
            }
            puzzle.NumberOfPieces = numberOfPieces;
            puzzle.BrandName = BrandName.Schmidt;
            puzzle.ShortId = long.Parse(shortid) ;
            var images = doc.DocumentNode.SelectNodes("//figure/img");
            var imageUrl = GetBaseUrl(url) + "/" + images.First().GetAttributeValue("src","");
            puzzle.Name = name.CleanUpName();           
            puzzle.ImageUrl = imageUrl;
            return puzzle;
        }

        private async Task<byte[]> GetImage(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetByteArrayAsync(new Uri(url));
            }
        }

        private static List<string> GetAllLinksPerPage(string url)
        {
            var linksToPuzzles = new List<string>();

            var web = new HtmlWeb();
            var doc = web.Load(url);
            var referenceToJigsaws = doc.DocumentNode.SelectNodes($"//a[@class='button']");
            if (referenceToJigsaws != null && referenceToJigsaws.Count > 0)
            {
                foreach (var reference in referenceToJigsaws)
                {
                    var puzzlePageLink = reference.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(puzzlePageLink) || puzzlePageLink.StartsWith("/detail/product/"))
                    {
                        linksToPuzzles.Add(puzzlePageLink);
                    }
                }
            }

            return linksToPuzzles;

        }

        public bool SupportsBrand(BrandName brandName)
        {
            return brandName.Equals(BrandName.Schmidt);
        }
    }
}
