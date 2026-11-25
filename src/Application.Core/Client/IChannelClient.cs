using Application.Core.Channel;
using Application.Core.Game.Trades;
using Application.Shared.MapObjects.Players;
using scripting.npc;

namespace Application.Core.Client
{
    public interface IChannelClient : IClientBase
    {
        WorldChannel CurrentServer { get; }
        int Channel { get; }
        Player? Character { get; }
        Player OnlinedCharacter { get; }
        /// <summary>
        /// 当前角色 id，不需要拿到 <see cref="Player"/> 也能用。
        /// <para>
        /// 给拍卖行这类「玩家已经脱离世界、不该再碰 Player」的逻辑用的：
        /// 它们只需要身份 + 发包，不需要背包 / 现金这些状态。
        /// </para>
        /// </summary>
        int CharacterId { get; }
        ITradeServer? TradeServer { get; set; }
        IClientCulture CurrentCulture { get; set; }

        NPCConversationManager? NPCConversationManager { get; set; }
        Task Disconnect(bool isShutdown, bool fromCashShop = false);

        bool GainCharacterSlot();
        void SetPlayer(Player? player);
        void SetAccount(AccountInfoModel accountEntity);
        WorldChannel getChannelServer();
        int getChannel();


        bool CheckBirthday(DateTime date);
        bool CheckBirthday(int date);
        Task LeaveCashShop();
        Task ChangeChannel(int channel);
    }
}
