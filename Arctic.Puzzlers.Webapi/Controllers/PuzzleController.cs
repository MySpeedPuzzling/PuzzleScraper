using Arctic.Puzzlers.Objects.PuzzleObjects;
using Arctic.Puzzlers.Stores;
using Microsoft.AspNetCore.Mvc;

namespace Arctic.Puzzlers.Worker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PuzzleController : ControllerBase
    {
        private readonly IPuzzleStore m_store;

        public PuzzleController(IPuzzleStore store) 
        { 
            m_store=store;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PuzzleExtended>>> Get()
        { 
            return await m_store.GetAll();
        }
    }
}
