using Application.Host.Models;
using Application.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Host.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        readonly DropdataService _dropService;

        public GameController(DropdataService dropService)
        {
            _dropService = dropService;
        }

        [HttpPut]
        public async Task<int> PutDropData([FromBody] DropDataDto data)
        {
            return await _dropService.AddOrUpdate(data);
        }

        [HttpGet]
        public async Task<PagedData<DropDataDto>> GetPagedDropData([FromQuery] DropdataFilter filter)
        {
            return await _dropService.GetPagedData(filter);
        }
    }
}
