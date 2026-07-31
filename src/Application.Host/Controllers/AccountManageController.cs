using Application.Core.Login;
using Application.Core.Login.Dtos.Account;
using Application.Core.Login.Dtos.Ban;
using Application.Core.Login.Dtos.Character;
using Application.Host.Middlewares;
using Application.Host.Models;
using Microsoft.AspNetCore.Mvc;

namespace Application.Host.Controllers
{
    [UserAuthorize(Roles = "Admin")]
    public class AccountManageController: BaseApiController
    {
        readonly MasterServer _server;

        public AccountManageController(MasterServer server)
        {
            _server = server;
        }

        /// <summary>
        /// 分页加载账号信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public PagedData<AccountResponseDto> GetAccountPagedData([FromQuery]AccountQuery query)
        {
            var (data, count) = _server.AccountManager.GetAccountPagedData(query.PageIndex, query.PageSize);
            return new PagedData<AccountResponseDto>(data, count);
        }

        /// <summary>
        /// 加载账号详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public AccountDetailDto? GetAccountDetail(int id)
        {
            return _server.AccountManager.GetAccountDetailAsync(id);
        }

        /// <summary>
        /// 分页加载账号封禁信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public PagedData<BanResponseDto> GetAccountBanPagedData([FromQuery] AccountQuery query)
        {
            var (data, count) = _server.AccountBanManager.GetBanPagedData(query.Ban, query.PageIndex, query.PageSize);
            return new PagedData<BanResponseDto>(data, count);
        }

        /// <summary>
        /// 封禁账号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public Task<bool> BanAccount([FromBody] BanRequestDto model)
        {
            var endTime = _server.GetCurrentTimeDateTimeOffset().AddHours(model.Hours);
            return _server.AccountBanManager.BanAccount(User.Identity.GetUserId(), model.TargetAccountId, endTime, (int)model.BanLevel, (int)model.Reason, model.ReasonDesc);
        }

        /// <summary>
        /// 撤销某次封禁
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public Task<bool> RevokeAccountBan([FromBody] UnbanRequestDto model)
        {
            return _server.AccountBanManager.UnbanAccount(User.Identity.GetUserId(), model.TargetAccountId);
        }
    }
}
