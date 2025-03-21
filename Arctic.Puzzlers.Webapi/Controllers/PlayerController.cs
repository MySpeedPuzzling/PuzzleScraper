using Arctic.Puzzlers.Objects.DTO;
using Arctic.Puzzlers.Webapi.DB;
using Microsoft.AspNetCore.Mvc;

namespace Arctic.Puzzlers.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerDBStore m_store;
        private readonly ILogger<PlayerController> m_logger;

        public PlayerController(IPlayerDBStore playerDBStore, ILogger<PlayerController> logger)
        {
            m_store = playerDBStore;
            m_logger = logger;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerDTO>>> Get([FromQuery] string name)
        {
            try
            {
                return Ok(await m_store.SearchByNameAsync(name));
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to get players by name");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<IEnumerable<PlayerDTO>>> GetById(Guid id)
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
                m_logger.LogError(ex, "Failed to get player by id");
                return StatusCode(500);
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<PlayerDTO>> CreateOrUpdatePlayer(Guid id, [FromBody] PlayerDTO player)
        {
            try
            {
                var foundPlayer = await m_store.GetByIdAsync(id);
                if (foundPlayer == null)
                {
                    
                    await m_store.InsertAsync(player);
                    // TODO: Add some logic to update player data
                    return Created();
                }
                else
                {
                    player.Id = foundPlayer.Id;
                    await m_store.UpdateAsync(player);
                    return Ok(player);
                }
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to create or update player");
                return StatusCode(500);
            }
        }
    }   
}
