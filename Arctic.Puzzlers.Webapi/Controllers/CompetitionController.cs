using Arctic.Puzzlers.Objects.CompetitionObjects;
using Arctic.Puzzlers.Stores;
using Microsoft.AspNetCore.Mvc;

namespace Arctic.Puzzlers.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionController : ControllerBase
    {
        private readonly ICompetitionStore m_store;
        public CompetitionController(ICompetitionStore competitionStore) 
        {
            m_store = competitionStore;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Competition>>> Get()
        {
            return await m_store.GetAll();
        }
    }
}
