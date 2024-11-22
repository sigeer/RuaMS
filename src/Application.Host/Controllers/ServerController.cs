using Application.Host.Models;
using Application.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Host.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        readonly DataService _dataService;
        readonly ServerService _serverService;
        readonly GameHost _serverHost;
        public ServerController(GameHost gameHost, DataService dataService, ServerService serverService)
        {
            _serverHost = gameHost;
            _dataService = dataService;
            _serverService = serverService;
        }

        [HttpGet]
        public ServerInfoDto GetServerInfo()
        {
            return _serverService.GetServerInfo();
        }

        [HttpPost]
        public async Task<bool> StartServer(bool ignoreCache = false)
        {
            await _serverHost.StartNow(ignoreCache);
            return true;
        }

        [HttpPost]
        public async Task<bool> StopServer()
        {
            await _serverHost.StopNow();
            return true;
        }

        [HttpPut]
        public async Task<bool> ApplyWorldSetting(int id)
        {
            return await _serverService.Apply(id);
        }

        [HttpPut]
        public async Task<bool> ApplyAllWorldSetting()
        {
            return await _serverService.Apply();
        }

        [HttpGet]
        public List<KeyValuePair<int, string>> LoadWorlds()
        {
            return _dataService.GetWorldsData();
        }

        [HttpGet]
        public List<WorldServerDto> GetWorldServerList()
        {
            return _serverService.GetWorldServerList();
        }

        [HttpPut]
        public async Task<bool> PutWorldConfig([FromBody]WorldServerConfig data)
        {
            return await _serverService.UpdateConfig(data);
        }

        [HttpPut]
        public async Task<bool> ToggleWorldState([FromBody]WorldServerState data)
        {
            return await _serverService.ToggleWorldServerState(data);
        }
    }
}
