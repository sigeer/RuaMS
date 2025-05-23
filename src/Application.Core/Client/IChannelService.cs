
using Application.Core.Datas;
using Application.Shared.Dto;
using client.inventory;
using server;

namespace Application.Core.Client
{
    public interface IChannelService
    {
        Item GenerateCouponItem(int itemId, short quantity);
        void RemovePlayerIncomingInvites(int id);
        void SaveChar(Player player);
        Item CashItem2Item(CashShop.CashItem cashItem);
        void SaveBuff(IPlayer player);
        PlayerBuffSaveDto GetBuffFromStorage(IPlayer player);
        /// <summary>
        /// 获取一个正在准备进入游戏的CharacterValueObject
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        CharacterValueObject? GetPlayerData(string clientSession, int cid);
    }
}
