using Arctic.Puzzlers.Objects.DTO;
using Arctic.Puzzlers.Webapi.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arctic.Puzzlers.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionGroupController : ControllerBase
    {
        private readonly ICompetitionGroupDBStore m_store;
        private readonly ILogger<CompetitionGroupController> m_logger;
        public CompetitionGroupController(ICompetitionGroupDBStore competitionGroupDBStore,ILogger<CompetitionGroupController> logger)
        {
            m_store = competitionGroupDBStore;
            m_logger = logger;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<CompetitionGroupDTO>> GetById(Guid id)
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
                m_logger.LogError(ex, "Failed to get competition group by id");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("competition/{competitionid}")]
        public async Task<ActionResult<IEnumerable<CompetitionGroupDTO>>> GetByCompetitionId(Guid competitionid)
        {
            try
            {
                return Ok(await m_store.GetByCompetitionIdAsync(competitionid));
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to get competition groups by competition id");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> CreateOrUpdateCompetitionGroup(Guid id, [FromBody] CompetitionGroupDTO competitionGroup)
        {
            try
            {
                var foundCompetitionGroup = await m_store.GetByIdAsync(id);
                
                if (foundCompetitionGroup != null)    
                {
                    competitionGroup.Id = foundCompetitionGroup.Id;
                    if (!await m_store.Update(competitionGroup))
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                    return Ok(competitionGroup);
                }
                var foundCompetitionGroupWithType = await m_store.GetByCompetitionIdAndType(competitionGroup.CompetitionId, competitionGroup.ContestType);
                if (foundCompetitionGroupWithType != null)
                {
                    competitionGroup.Id = foundCompetitionGroupWithType.Id;
                    if (!await m_store.Update(competitionGroup))
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                    return Ok(competitionGroup);
                }
                if (!await m_store.Insert(competitionGroup))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                return Ok(competitionGroup);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to create or update competition group");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
