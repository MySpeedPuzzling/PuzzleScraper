using Arctic.Puzzlers.Objects.DTO;
using Arctic.Puzzlers.Objects.Misc;
using Arctic.Puzzlers.Webapi.DB;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Arctic.Puzzlers.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultController : ControllerBase
    {
        private readonly IResultDBStore m_store;
        private readonly ILogger<ResultController> m_logger;
        private readonly ICompetitionDBStore m_competitionDbStore;
        private readonly ICompetitionGroupDBStore m_competitionGroupDBStore;
        private readonly IPlayerDBStore m_playerDbStore;
        private readonly ICompetitionRoundDBStore m_competitionRoundDbStore;
        public ResultController(IResultDBStore resultDBStore, IPlayerDBStore playerDBStore, ICompetitionRoundDBStore competitionRoundDBStore, ICompetitionGroupDBStore competitionGroupDBStore, ICompetitionDBStore competitionDBStore, ILogger<ResultController> logger)
        {
            m_store = resultDBStore;
            m_playerDbStore = playerDBStore;
            m_competitionRoundDbStore = competitionRoundDBStore;
            m_logger = logger;
            m_competitionDbStore = competitionDBStore;
            m_competitionGroupDBStore = competitionGroupDBStore;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<ResultDTO>> GetById(Guid id)
        {
            try
            {
                var response = await m_store.GetByIdAsync(id);
                if (response != null)
                {
                    return Ok(response);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to get result by id");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("player/{id}")]
        public async Task<ActionResult<IEnumerable<ResultDTO>>> GetByPlayerId(Guid id)
        {
            try
            {
                return Ok(await m_store.GetByPlayerIdAsync(id));
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to get results by player id");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("competitionround/{id}")]
        public async Task<ActionResult<IEnumerable<ResultDTO>>> GetByCompetitonRoundId(Guid roundId)
        {
            try
            {
                return Ok(await m_store.GetByRoundIdAsync(roundId));
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to get results by round id");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("Simplified")]
        public async Task<ActionResult> GetCsv([FromQuery] Guid competitionRoundId, [FromQuery] string format)
        {


            var results = await m_store.GetByRoundIdAsync(competitionRoundId);
            var rounddata = await m_competitionRoundDbStore.GetByIdAsync(competitionRoundId);
            if (rounddata == null)
            {
                return NotFound();
            }
            var groupData = await m_competitionGroupDBStore.GetByIdAsync(rounddata.CompetitionGroupId);
            if(groupData == null)
            {
                return NotFound();
            }

            var competition = await m_competitionDbStore.GetByIdAsync(groupData.CompetitionId);
            if(competition == null)
            {
                return NotFound();
            }
            var competitionCSVs = new List<CompetitionResult>();

            foreach (var result in results)
            {
                var playersName = string.Empty;
                foreach (var playerId in result.PlayerIds) {
                    var player = await m_playerDbStore.GetByIdAsync(playerId);
                
                    playersName = string.Join(";", player.FullName);
                }
                if (!string.IsNullOrEmpty(result.GroupName))
                {
                    playersName = result.GroupName + "(" + playersName + ")";
                }

                var competitionCSV = new CompetitionResult()
                {
                    Date = rounddata.Time,
                    EventName = competition.Name + " " + groupData.ContestType + " " + rounddata.RoundName,
                    PlayersName = playersName,
                    Rank = result.Rank,
                    PieceCount = result.FinishedPieces,
                    Time = result.TotalTime
                };

                competitionCSVs.Add(competitionCSV);
            }

            if (rounddata.MaxTime != TimeSpan.Zero)
            {
                int playerCounter = 0;
                double totalPiecesPerMinute = 0.0;
                competitionCSVs.ForEach(compCSV =>
                {
                    var timespent = compCSV.Time == TimeSpan.Zero ? rounddata.MaxTime : compCSV.Time;
                    var piecesPerMinute = (compCSV.PieceCount / timespent.TotalSeconds) * 60;
                    compCSV.PiecesPerMinute = Math.Round(piecesPerMinute, 2);
                    if (piecesPerMinute > 0)
                    {
                        totalPiecesPerMinute += piecesPerMinute;
                        playerCounter++;
                    }

                });

                var averagePiecesPerMinute = totalPiecesPerMinute / playerCounter;
                if (averagePiecesPerMinute > 0)
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


        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<ResultDTO>> CreateOrUpdate(Guid id, [FromBody] ResultDTO result)
        {
            try
            {
                var foundResult = await m_store.GetByIdAsync(id);
                if (foundResult == null)
                {
                    foreach(var playerId in result.PlayerIds)
                    {
                        var playerResult = await m_store.GetByPlayerIdAndRoundIdAsync(playerId,result.CompetitionRoundId);
                        if(playerResult != null)
                        {
                            result.Id = playerResult.Id;
                            await m_store.UpdateAsync(result);
                            return Ok(result);
                        }
                    }                    

                    await m_store.InsertAsync(result);
                    return Created();
                }
                else
                {
                    result.Id = foundResult.Id;
                    await m_store.UpdateAsync(result);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to create or update result");
                return StatusCode(500);
            }
        }


    }
}
