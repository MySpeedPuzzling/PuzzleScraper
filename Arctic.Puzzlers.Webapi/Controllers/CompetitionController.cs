using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Stores;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using CsvHelper;

namespace Arctic.Puzzlers.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionController : ControllerBase
    {
        private readonly ICompetitionStore m_store;
        private readonly IConfiguration m_configuration;

        public CompetitionController(ICompetitionStore competitionStore, IConfiguration configuration) 
        {
            m_store = competitionStore;
            m_configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Competition>>> Get()
        {
            return await m_store.GetAll();
        }

        [HttpGet]
        [Route("CSV/ListOfCompetitions")]
        public async Task<ContentResult> GetAllCompetitions()
        {
            var html = "<p>List of competitions</p>";
            var results = await m_store.GetAll();

            var baseurl= m_configuration.GetValue("BASEURL",string.Empty);
            if (!baseurl.EndsWith('/'))
            {
                baseurl += "/";
            }
            foreach (var result in results)
            {
                foreach (var competitionGroups in result.CompetitionGroups)
                {
                    foreach (var round in competitionGroups.Rounds) {
                        html += "<br/> ";
                        html += $"<a href=\"{baseurl}/api/Competition/CSV?Competitionname={result.Name}&RoundName={competitionGroups.ContestType + "-" + round.RoundName} \">{result.Name + " " + competitionGroups.ContestType + " " + round.RoundName} </a>";
                    }
                }
               
            }
            return new ContentResult
            {
                Content = html,
                ContentType = "text/html"
            };
        }


        [HttpGet]
        [Route("CSV")]
        [Produces("text/csv")]
        public async Task<FileResult> GetCsv(string Competitionname, string RoundName)
        {
            
            var data = await m_store.GetByName(Competitionname);
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecords(data);
                }

                return File(memoryStream.ToArray(), "text/csv", $"Export-{DateTime.Now.ToString("s")}.csv");
            }
        }

    }
}
