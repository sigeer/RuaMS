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
        readonly DataService _dataService;

        public GameController(DropdataService dropService, DataService dataService)
        {
            _dropService = dropService;
            _dataService = dataService;
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

        [HttpDelete]
        public async Task<int> DeleteDropData([FromQuery] int id)
        {
            return await _dropService.Delete(id);
        }

        [HttpGet]
        public async Task<PagedData<DropDataDto>> GetGloablPagedData([FromQuery] DropdataFilter filter)
        {
            return await _dropService.GetGloablPagedData(filter);
        }

        [HttpPut]
        public async Task<int> PutGlobalDropData([FromBody] DropDataDto data)
        {
            return await _dropService.AddOrUpdateGlobal(data);
        }

        [HttpDelete]
        public async Task<int> DeleteGlobalDropData([FromQuery] int id)
        {
            return await _dropService.DeleteGlobal(id);
        }


        /// <summary>
        /// 自动完成 - 怪物查找
        /// </summary>
        /// <param name="mob"></param>
        /// <returns></returns>
        [HttpGet]
        public List<KeyValuePair<int, string>> FilterMonster(string? mob)
        {
            return _dataService.FilterMonster(mob);
        }

        /// <summary>
        /// 自动完成 - 道具
        /// </summary>
        /// <param name="mob"></param>
        /// <returns></returns>
        [HttpGet]
        public List<KeyValuePair<int, string>> FilterItem(string? item)
        {
            return _dataService.FilterItem(item);
        }

        [HttpGet]
        public List<KeyValuePair<int, string>> LoadWorlds()
        {
            return _dataService.GetWorldsData();
        }
    }
}
