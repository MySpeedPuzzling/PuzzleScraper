using Arctic.Puzzlers.Objects.DTO;
using Arctic.Puzzlers.Webapi.DB;
using Microsoft.AspNetCore.Mvc;

namespace Arctic.Puzzlers.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionRoundController : ControllerBase
    {
        private readonly ICompetitionRoundDBStore m_store;
        private readonly ILogger<CompetitionRoundController> m_logger;
        public CompetitionRoundController(ICompetitionRoundDBStore competitionRoundDBStore, ILogger<CompetitionRoundController> logger)
        {
            m_store = competitionRoundDBStore;
            m_logger = logger;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<CompetitionRoundDTO>> GetById(Guid id)
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
                m_logger.LogError(ex, "Failed to get competition round by id");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("competitiongroup/{id}")]
        public async Task<ActionResult<IEnumerable<CompetitionRoundDTO>>> GetByCompetitionGroupId(Guid id)
        {
            try
            {
                return Ok(await m_store.GetByGroupId(id));
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to get competition rounds by competition group id");
                return StatusCode(500);
            }
        }
                

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<CompetitionRoundDTO>> CreateOrUpdateCompetitionRound(Guid id, [FromBody] CompetitionRoundDTO competitionRound)
        {
            try
            {
                var foundCompetitionRound = await m_store.GetByIdAsync(id);
                if (foundCompetitionRound != null)
                {
                    if (foundCompetitionRound.Id != competitionRound.Id)
                    {
                        return BadRequest("Id in the request body does not match the id in the URL");
                    }
                    competitionRound.Id = foundCompetitionRound.Id;
                    if (await m_store.Update(competitionRound))
                    {
                        return Ok(competitionRound);
                    }
                }

                var foundCompetitionRoundByNameAndGroupId = await m_store.GetByNameAndGroupId(competitionRound.RoundName, competitionRound.CompetitionGroupId);

                if (foundCompetitionRoundByNameAndGroupId != null)
                {
                    if (await m_store.Update(competitionRound))
                    {
                        return Ok(competitionRound);
                    }
                }
                if (await m_store.Insert(competitionRound))
                {
                    return Created();
                }
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to create or update competition round");
                return StatusCode(500);
            }
        }
    }
}
