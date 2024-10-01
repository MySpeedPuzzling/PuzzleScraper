using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arctic.Puzzlers.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly ICompetitionStore m_store;
        public PlayerController(ICompetitionStore competitionStore)
        {
            m_store = competitionStore;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerCompetitionResult>>> Get(string name)
        {
            return await m_store.GetPlayerCompetitionResultByName(name);
        }
    }   
}
