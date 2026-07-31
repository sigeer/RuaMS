using Application.Host.Middlewares;
using Application.Host.Models;
using Application.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Host.Controllers
{
    [UserAuthorize(Roles = "Admin")]
    public class ServerController : BaseApiController
    {

        readonly ServerService _serverService;

        public ServerController(ServerService serverService)
        {
            _serverService = serverService;
        }

        /// <summary>
        /// 服务器状态面板
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ServerDashboard GetDashboard()
        {
            return _serverService.GetDashboard();
        }
    }
}
