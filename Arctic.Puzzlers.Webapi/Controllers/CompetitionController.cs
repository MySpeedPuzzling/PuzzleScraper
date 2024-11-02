using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Stores;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using CsvHelper;
using Arctic.Puzzlers.Objects.Misc;
using Microsoft.VisualBasic;

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
                        html += $"<a href=\"{baseurl}api/Competition/CSV?competition={result.CompetitionId}&round={round.RoundId} \">{result.Name + " " + competitionGroups.ContestType + " " + round.RoundName} </a>";
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
        public async Task<ActionResult> GetCsv(Guid competition, Guid round)
        {
            
            var data = await m_store.Get(competition);
            if(data == null)
            {
                return NotFound();
            }
            var groupData = data.CompetitionGroups.Where(t => t.Rounds.Any(t => t.RoundId == round)).FirstOrDefault();
            if (groupData == null)
            {
                return NotFound();
            }
            var rounddata = groupData.Rounds.Where(t => t.RoundId == round).FirstOrDefault();
            if(rounddata == null)
            {
                return NotFound();
            }
            var competitionCSVs = new List<CompetitionCSV>();

            foreach(var result in rounddata.Participants)
            {
                var playersName = string.Empty;
                playersName = string.Join(";", result.Participants.Select(t=>t.FullName));
                if (!string.IsNullOrEmpty(result.GroupName))
                {
                    playersName = result.GroupName + "(" + playersName + ")";
                }
                var competitionCSV = new CompetitionCSV()
                {
                    Date = rounddata.Time,
                    EventName = data.Name + " " + groupData.ContestType + " " + rounddata.RoundName,
                    PlayersName = playersName
                };

                competitionCSVs.Add(competitionCSV);
            }
            using (var memoryStream = new MemoryStream())
            {               
                using (var streamWriter = new StreamWriter(memoryStream))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecords(competitionCSVs);
                }

                return File(memoryStream.ToArray(), "text/csv", $"Export-{DateTime.Now.ToString("s")}.csv");
            }
        }

    }
}
