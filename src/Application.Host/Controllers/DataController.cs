using Application.Core.Login;
using Application.Core.Login.Dtos.Drop;
using Application.Core.Login.Dtos.Gachapon;
using Application.Core.Login.Dtos.Shop;
using Application.Host.Models;
using Application.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Host.Controllers
{

    public class DataController : BaseApiController
    {
        readonly MasterServer _server;
        readonly DataIdService _dataIdService;

        public DataController(MasterServer server, DataIdService dataIdService)
        {
            _server = server;
            _dataIdService = dataIdService;
        }
        /// <summary>
        /// 通过名称查询map
        /// </summary>
        /// <param name="mapName"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public List<IdName> Map(string mapName, string locale = "zh-CN") => _dataIdService.QueryMap(mapName, locale);
        /// <summary>
        /// 通过名称查询mob
        /// </summary>
        /// <param name="mobName"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public List<IdName> Mob(string mobName, string locale = "zh-CN") => _dataIdService.QueryMob(mobName, locale);
        /// <summary>
        /// 通过名称查询npc
        /// </summary>
        /// <param name="npcName"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public List<IdName> Npc(string npcName, string locale = "zh-CN") => _dataIdService.QueryNpc(npcName, locale);
        /// <summary>
        /// 通过名称查询物品
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public List<IdName> Item(string itemName, string locale = "zh-CN") => _dataIdService.QueryItem(itemName, locale);
        /// <summary>
        /// 通过名称查询任务
        /// </summary>
        /// <param name="questName"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public List<IdName> Quest(string questName, string locale = "zh-CN") => _dataIdService.QueryQuest(questName, locale);


        /// <summary>
        /// 分页加载怪物掉落
        /// </summary>
        /// <param name="query"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PagedData<DropResponseDto>> GetMobDropPagedData([FromQuery] MobDropQuery query, string locale = "zh-CN")
        {
            var (data, count) = _server.DropDataManager.QueryMobDrop(query.MobId, query.ItemId, query.QuestId, query.PageIndex, query.PageSize, locale);
            return new PagedData<DropResponseDto>(data, count);
        }


        /// <summary>
        /// 分页加载商店
        /// </summary>
        /// <param name="query"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PagedData<ShopResponseDto>> GetPagedShopData([FromQuery] ShopQuery query, string locale = "zh-CN")
        {
            var (data, count) = _server.ShopManager.GetPagedData(query.ItemId, query.PageIndex, query.PageSize, locale);
            return new PagedData<ShopResponseDto>(data, count);
        }

        /// <summary>
        /// 加载商店物品
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ShopDetailDto?> GetShopItems(int shopId, string locale = "zh-CN")
        {
            return await _server.ShopManager.GetShopItems(shopId, locale);
        }


        /// <summary>
        /// 加载扭蛋机
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<GachaponResponseDto>> GetGachapons(int? itemId, string locale = "zh-CN")
        {
            return _server.GachaponManager.GetAllGachaponList(itemId, locale);
        }

        /// <summary>
        /// 加载扭蛋机详细
        /// </summary>
        /// <param name="id"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        [HttpGet]
        public GachaponDetailResponseDto GetGachaponDetail(int id, string locale = "zh-CN")
        {
            return _server.GachaponManager.GetGachaponDetail(id, locale);
        }
    }
}
