using Application.Core.Channel.DataProviders;
using Application.Core.Channel.DueyService;
using Application.Core.Channel.Modules;
using Application.Core.Channel.ServerData;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Application.Core.Channel.Services
{
    /// <summary>
    /// 只读服务
    /// </summary>
    public interface IServiceCenter
    {
        IItemDistributeService ItemDistributeService { get; }
        AdminService AdminService { get; }
        DataService DataService { get; }
        ItemService ItemService { get; }
        CrossServerCallbackService RemoteCallService { get; }
        ExpeditionService ExpeditionService { get; }
        IPlayerNPCService PlayerNPCService { get; }
        PlayerShopService PlayerShopService { get; }
        NewYearCardService NewYearCardService { get; }
        IMarriageService MarriageService { get; }
        IFishingService FishingService { get; }
        TeamManager TeamManager { get; }
        GuildManager GuildManager { get; }
        ShopManager ShopManager { get; }
        DueyManager DueyManager { get; }
        GachaponManager GachaponManager { get; }
        AutoBanDataManager AutoBanManager { get; }
        MonitorManager MonitorManager { get; }

        SkillbookInformationProvider SkillbookInformationProvider { get; }
        CashItemProvider CashItemProvider { get; }
        BatchSyncManager<int, SyncProto.MapSyncDto> BatchSynMapManager { get; }
        List<AbstractChannelModule> Modules { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">-1. yellow, -2. earntitle</param>
        /// <param name="message"></param>
        /// <param name="onlyGM"></param>
        /// <returns></returns>
        void SendDropMessage(int type, string message, bool onlyGM = false);
        void SendBroadcastWorldPacket(Packet p, bool onGM = false);
        /// <summary>
        /// 昵称是否可用
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool CheckCharacterName(string name);
        void SendReloadEvents(Player chr);
        IPEndPoint GetChannelEndPoint(int channel);
        List<int> GetActiveCoupons();
        Dictionary<int, int> GetCouponRates();
    }
}
