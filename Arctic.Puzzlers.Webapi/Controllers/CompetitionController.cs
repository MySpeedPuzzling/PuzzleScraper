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
        [Route("ListOfCompetitions")]
        public async Task<ContentResult> GetAllCompetitions()
        {
            var html = "<p>List of competitions</p>";
            var results = await m_store.GetAll();

            foreach (var result in results)
            {
                foreach (var competitionGroups in result.CompetitionGroups)
                {
                    foreach (var round in competitionGroups.Rounds) {
                        html += $"<p>{result.Name + " " + competitionGroups.ContestType + " " + round.RoundName} <a href=\"/api/Competition/Simplified?competition={result.CompetitionId}&round={round.RoundId}&format=csv \">CSV </a> <a href=\"/api/Competition/Simplified?competition={result.CompetitionId}&round={round.RoundId}&format=json \">JSON</a></p>";
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
        [Route("Simplified")]
        public async Task<ActionResult> GetCsv(Guid competition, Guid round, string format)
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
            var competitionCSVs = new List<CompetitionResult>();

            foreach(var result in rounddata.Participants)
            {
                var playersName = string.Empty;
                playersName = string.Join(";", result.Participants.Select(t=>t.FullName));
                if (!string.IsNullOrEmpty(result.GroupName))
                {
                    playersName = result.GroupName + "(" + playersName + ")";
                }
                
                var competitionCSV = new CompetitionResult()
                {
                    Date = rounddata.Time,
                    EventName = data.Name + " " + groupData.ContestType + " " + rounddata.RoundName,
                    PlayersName = playersName,
                    Rank = result.Rank,
                    PieceCount = result.FinishedPieces,
                    Time = result.TotalTime                    
                };
                
                competitionCSVs.Add(competitionCSV);
            }

            if(rounddata.MaxTime != TimeSpan.Zero)
            {
                int playerCounter = 0;
                double totalPiecesPerMinute = 0.0;
                competitionCSVs.ForEach(compCSV =>
                {
                    var timespent = compCSV.Time == TimeSpan.Zero ? rounddata.MaxTime : compCSV.Time;
                    var piecesPerMinute = (compCSV.PieceCount / timespent.TotalSeconds) * 60;
                    compCSV.PiecesPerMinute = Math.Round(piecesPerMinute, 2);
                    if(piecesPerMinute >0)
                    {
                        totalPiecesPerMinute += piecesPerMinute;
                        playerCounter++;
                    }

                });

                var averagePiecesPerMinute = totalPiecesPerMinute / playerCounter;
                if(averagePiecesPerMinute > 0)
                {
                    competitionCSVs.ForEach(compCSV =>
                    {
                        compCSV.SimplifiedJpar = Math.Round(averagePiecesPerMinute / compCSV.PiecesPerMinute, 8);
                    });
                }   
                
            }

            
            switch (format)
            {
                case "json":
                    return Ok(competitionCSVs);
                case "csv":
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var streamWriter = new StreamWriter(memoryStream))
                        using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                        {
                            csvWriter.WriteRecords(competitionCSVs);
                        }

                        return File(memoryStream.ToArray(), "text/csv", $"Export-{DateTime.Now.ToString("s")}.csv");
                    }
                default:
                    return Ok(competitionCSVs);
            }

        }

    }
}
