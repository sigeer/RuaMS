using Application.Host.Models;
using Application.Host.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Host.Controllers
{
#if !DEBUG
    [Authorize]
#endif
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        readonly AuthService _authService;
        readonly DataService _dataService;
        readonly ServerService _serverService;
        readonly GameHost _serverHost;
        public ServerController(AuthService authService, GameHost gameHost, DataService dataService, ServerService serverService)
        {
            _authService = authService;
            _serverHost = gameHost;
            _dataService = dataService;
            _serverService = serverService;
        }

        [AllowAnonymous]
        [HttpPost]
        public string Auth([FromBody] AuthModel model)
        {
            return _authService.CheckAuth(model.Password);
        }

        [HttpGet]
        public string Refresh()
        {
            return _authService.GenerateToken();
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
        public bool ApplyWorldSetting(int id)
        {
            return _serverService.Apply(id);
        }

        [HttpPut]
        public bool ApplyAllWorldSetting()
        {
            return _serverService.Apply();
        }

        [HttpGet]
        public List<KeyValuePair<int, string>> LoadWorlds()
        {
            return _dataService.GetWorldsData();
        }

        //[HttpGet]
        //public List<WorldServerDto> GetWorldServerList()
        //{
        //    return _serverService.GetWorldServerList();
        //}

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
