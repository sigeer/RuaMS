using Application.Core.Login;
using Application.Core.Login.Dtos.Character;
using Application.Host.Middlewares;
using Application.Host.Models;
using Microsoft.AspNetCore.Mvc;

namespace Application.Host.Controllers
{
    [UserAuthorize(Roles = "Admin")]
    public class CharacterController : BaseApiController
    {
        MasterServer _server;

        public CharacterController(MasterServer server)
        {
            _server = server;
        }

        /// <summary>
        /// 分页加载角色
        /// </summary>
        /// <param name="online">是否在线</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public PagedData<CharacterResponseDto> GetCharacterPagedData(int online, int pageIndex, int pageSize)
        {
            var (data, count) = _server.CharacterManager.GetCharacterPagedData(online, pageIndex, pageSize);
            return new PagedData<CharacterResponseDto>(data, count);
        }
    }
}
