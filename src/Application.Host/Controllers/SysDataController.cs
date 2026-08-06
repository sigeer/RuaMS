using Application.Core.Login;
using Application.Core.Login.Dtos.CDK;
using Application.Core.Login.Dtos.Drop;
using Application.Core.Login.Dtos.Gachapon;
using Application.Core.Login.Dtos.Report;
using Application.Core.Login.Dtos.Shop;
using Application.Core.Login.Services;
using Application.Host.Middlewares;
using Application.Host.Models;
using Application.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Host.Controllers
{
    [UserAuthorize(Roles = "Admin")]
    public class SysDataController : BaseApiController
    {
        readonly MasterServer _server;
        readonly ReportService _reportService;

        public SysDataController(MasterServer server, ReportService reportService)
        {
            _server = server;
            _reportService = reportService;
        }

        /// <summary>
        /// 更新或新增怪物掉落设置
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> SubmitMobDrop([FromBody] DropRequestDto data)
        {
            await _server.DropDataManager.SubmitMobDropData(data);
            return true;
        }

        /// <summary>
        /// 删除怪物掉落设置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> RemoveMobDrop([FromQuery] int id)
        {
            await _server.DropDataManager.DeleteDropData(id);
            return true;
        }

        /// <summary>
        /// 更新或新增全局掉落设置
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> SubmitGlobalDrop([FromBody] DropRequestDto data)
        {
            await _server.DropDataManager.SubmitGlobalDropData(data);
            return true;
        }

        /// <summary>
        /// 删除全局掉落设置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> RemoveGlobalDrop([FromQuery] int id)
        {
            await _server.DropDataManager.DeleteGlobalDropData(id);
            return true;
        }

        /// <summary>
        /// 更新或新增反应堆掉落设置
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> SubmitReactorDrop([FromBody] DropRequestDto data)
        {
            await _server.DropDataManager.SubmitReactorDropData(data);
            return true;
        }

        /// <summary>
        /// 删除反应堆掉落设置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> RemoveReactorDrop([FromQuery] int id)
        {
            await _server.DropDataManager.DeleteReactorDropData(id);
            return true;
        }


        /// <summary>
        /// 分页加载全局掉落
        /// </summary>
        /// <param name="query"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PagedData<DropResponseDto>> GetGlobalDropPagedData([FromQuery] GlobalDropQuery query, string locale = "zh-CN")
        {
            var (data, count) = _server.DropDataManager.QueryGlobalDrop(query.ContinentId, query.ItemId, query.QuestId, query.PageIndex, query.PageSize, locale);
            return new PagedData<DropResponseDto>(data, count);
        }


        /// <summary>
        /// 分页加载反应堆掉落
        /// </summary>
        /// <param name="query"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PagedData<DropResponseDto>> GetReactorDropPagedData([FromQuery] ReactorDropQuery query, string locale = "zh-CN")
        {
            var (data, count) = _server.DropDataManager.QueryReactorDrop(query.ItemId, query.QuestId, query.PageIndex, query.PageSize, locale);
            return new PagedData<DropResponseDto>(data, count);
        }

        /// <summary>
        /// 更新商店
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public bool SubmitShop([FromBody] CreateShopRequestDto data)
        {
            _server.ShopManager.SubmitShop(data);
            return true;
        }


        /// <summary>
        /// 移除商店
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> RemoveShop([FromQuery] int id)
        {
            await _server.ShopManager.DeleteShop(id);
            return true;
        }

        /// <summary>
        /// 更新商店物品
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> SubmitShopItem([FromBody] EditShopItemRequestDto data)
        {
            await _server.ShopManager.SubmitShopItem(data);
            return true;
        }

        /// <summary>
        /// 移除商店
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> RemoveShopItem([FromQuery] int id)
        {
            await _server.ShopManager.DeleteShopItem(id);
            return true;
        }

        /// <summary>
        /// 更新扭蛋机触发NPC
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> SubmitGachapon([FromBody] GachaponRequestDto data)
        {
            await _server.GachaponManager.Submit(data);
            return true;
        }

        /// <summary>
        /// 移除扭蛋机
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> RemoveGachapon([FromQuery] int id)
        {
            await _server.GachaponManager.Remove(id);
            return true;
        }

        /// <summary>
        /// 更新扭蛋机奖励
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> SubmitGachaponItem([FromBody] GachaponItemRequestDto data)
        {
            await _server.GachaponManager.SubmitReward(data);
            return true;
        }

        /// <summary>
        /// 移除扭蛋机奖励
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> RemoveGachaponItem(int id)
        {
            await _server.GachaponManager.RemoveReward(id);
            return true;
        }


        /// <summary>
        /// 分页加载Reward
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expired"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PagedData<RewardResponseDto>> GetRewardPagedData([FromQuery] Pagination query, int expired, int itemId)
        {
            var (data, count) = _server.RewardManager.GetPagedRewardAsync(expired, itemId, query.PageIndex, query.PageSize);
            return new PagedData<RewardResponseDto>(data, count);
        }

        /// <summary>
        /// 提交Reward
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> SubmitReward([FromBody] RewardDetailRequestDto data)
        {
            return await _server.RewardManager.SubmitReward(data);
        }

        /// <summary>
        /// 移除Reward
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> RemoveReward(int id)
        {
            await _server.RewardManager.RemoveReward(id);
            return true;
        }


        /// <summary>
        /// 获取Reward详细：奖品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<RewardDetailResponseDto?> GetRewardDetail(int id, string locale = "zh-CN") => _server.RewardManager.GetRewardDetail(id, locale);

        /// <summary>
        /// 获取Reward被使用情况
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<List<RewardRecordResponseDto>> GetRewardRecords(int id) => _server.RewardManager.GetRewardRecords(id);


        /// <summary>
        /// 分页加载玩家举报信息
        /// </summary>
        /// <param name="query"></param>
        /// <param name="processed"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PagedData<ReportResponseDto>> GetReportPagedData([FromQuery] Pagination query, int processed)
        {
            var (data, count) = _reportService.GetReportPagedData(processed, query.PageIndex, query.PageSize);
            return new PagedData<ReportResponseDto>(data, count);
        }

        /// <summary>
        /// 获取系统设置相关数据是否被改动
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public DirtyInfo GetDirtyInfo()
        {
            return new DirtyInfo
            {
                Shop = _server.ShopManager.IsDirty,
                Drop = _server.DropDataManager.IsDirty,
                Gachapon = _server.GachaponManager.IsDirty
            };
        }

        /// <summary>
        /// 通知频道服务器拉取最新数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> FlushData(string type)
        {
            if (type.Equals(nameof(DirtyInfo.Shop), StringComparison.OrdinalIgnoreCase))
            {
                await _server.ShopManager.FlushData();
            }
            else if (type.Equals(nameof(DirtyInfo.Drop), StringComparison.OrdinalIgnoreCase))
            {
                await _server.DropDataManager.FlushData();
            }
            else if (type.Equals(nameof(DirtyInfo.Gachapon), StringComparison.OrdinalIgnoreCase))
            {
                await _server.GachaponManager.FlushData();
            }
            return true;
        }
    }
}
