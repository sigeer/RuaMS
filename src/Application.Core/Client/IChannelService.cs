using Application.Core.Game.Life;
using Application.Shared.Dto;
using Application.Shared.Items;
using client.inventory;

namespace Application.Core.Client
{
    public interface IChannelService
    {
        Item GenerateCouponItem(int itemId, short quantity);
        void RemovePlayerIncomingInvites(int id);
        void SaveChar(Player player, bool isLogoff = false);
        Item CashItem2Item(CashItem cashItem);
        void SaveBuff(IPlayer player);
        PlayerBuffSaveDto GetBuffFromStorage(IPlayer player);
        /// <summary>
        /// 获取一个正在准备进入游戏的CharacterValueObject
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        CharacterValueObject? GetPlayerData(string clientSession, int cid);
        void SetPlayerOnlined(int id);
        Dictionary<int, List<DropEntry>> RequestAllReactorDrops();
    }
}
