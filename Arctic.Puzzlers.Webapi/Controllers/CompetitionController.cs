using Microsoft.AspNetCore.Mvc;
using Arctic.Puzzlers.Webapi.DB;
using Arctic.Puzzlers.Objects.DTO;

namespace Arctic.Puzzlers.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionController : ControllerBase
    {
        private readonly ICompetitionDBStore m_store;
        private readonly ICompetitionGroupDBStore m_groupStore;
        private readonly ICompetitionRoundDBStore m_roundStore;
        private readonly ILogger<CompetitionController> m_logger;
        public CompetitionController(ICompetitionDBStore store,ICompetitionGroupDBStore competitionGroupDBStore, ICompetitionRoundDBStore competitionRoundDBStore, ILogger<CompetitionController> logger) 
        {
            m_store = store;
            m_logger = logger;
            m_groupStore = competitionGroupDBStore;
            m_roundStore = competitionRoundDBStore; 
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<CompetitionDTO>> GetById(Guid id)
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
                m_logger.LogError(ex, "Failed to get competiton by id");
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<ActionResult<CompetitionDTO>> GetByName([FromQuery] string name)
        {
            try
            {
                var response = await m_store.GetByUniqueNameAsync(name);
                if (response != null)
                {
                    return Ok(response);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to get competiton by id");
                return StatusCode(500);
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> CreateOrUpdateCompetition(Guid id, [FromBody] CompetitionDTO competition)
        {
            try
            {
                var foundCompetition = await m_store.GetByIdAsync(id);
                
                if (foundCompetition == null)
                {
                    var foundCompetitionByName = await m_store.GetByUniqueNameAsync(competition.UniqueName);
                    if(foundCompetitionByName != null)
                    {
                        competition.Id = foundCompetitionByName.Id;
                        await m_store.UpdateAsync(competition);
                        return Ok(competition);
                    }
                    await m_store.InsertAsync(competition);
                    return Created();
                }
                competition.Id = foundCompetition.Id;
                await m_store.UpdateAsync(competition);
                return Ok(competition);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to create or update competition");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("ListOfCompetitions")]
        public async Task<ContentResult> GetAllCompetitions()
        {
            var html = "<p>List of competitions</p>";
            var competitions = await m_store.GetAllAsync();


            foreach (var competition in competitions)
            {
                foreach (var competitionGroup in await m_groupStore.GetByCompetitionIdAsync(competition.Id))
                {
                    foreach (var round in await m_roundStore.GetByGroupId(competitionGroup.Id)) {
                        html += $"<p>{competition.Name + " " + competitionGroup.ContestType + " " + round.RoundName} <a href=\"/api/Result/Simplified?competitionRoundId={round.Id}&format=csv \">CSV </a> <a href=\"/api/Result/Simplified?competitionRoundId={round.Id}&format=json \">JSON</a></p>";
                    }
                }
               
            }
            return new ContentResult
            {
                Content = html,
                ContentType = "text/html"
            };
        }   
    }
}
