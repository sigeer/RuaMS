using Application.Core.Game.Life;
using Application.Core.Game.TheWorld;
using Application.Core.Scripting.Infrastructure;
using Application.Shared.Models;
using scripting;
using scripting.Event;
using scripting.npc;

namespace Application.Core.Client
{
    public interface IChannelClient : IClientBase
    {
        IWorldChannel CurrentServer { get; }
        int Channel { get; }
        int ActualChannel { get; }
        /// <summary>
        /// 不和LoginClient一样使用AccountEntity的原因：LoginClient可以与MasterServer直接交互修改AccountEntity
        /// 而ChannelClient可能与MasterServer不在同一个进程，避免出现错误的以为能够直接修改AccountEntity
        /// </summary>
        AccountCtrl AccountEntity { get; set; }
        IPlayer? Character { get; }
        IPlayer OnlinedCharacter { get; }

        NPCConversationManager? NPCConversationManager { get; set; }
        EngineStorage ScriptEngines { get; set; }

        void Disconnect(bool isShutdown, bool fromCashShop = false);

        void enableCSActions();
        AbstractPlayerInteraction getAbstractPlayerInteraction();
        void lockClient();
        void unlockClient();
        /// <summary>
        /// 临时使用
        /// </summary>
        /// <returns></returns>
        IWorld getWorldServer();
        void OpenNpc(int npcid, string? script = null);
        bool canClickNPC();
        void setClickedNPC();
        void announceServerMessage();
        void closePlayerScriptInteractions();
        void announceHint(string msg, int length);
        bool CanGainCharacterSlot();
        bool GainCharacterSlot();
        void ChangeChannel(int channel);
        bool attemptCsCoupon();
        void resetCsCoupon();
        void SetPlayer(IPlayer? player);
        void SetAccount(AccountCtrl accountEntity);
        IWorldChannel getChannelServer();
        int getChannel();
        void announceBossHpBar(Monster mm, int mobHash, Packet packet);

        EventManager? getEventManager(string @event);
        void removeClickedNPC();

        bool CheckBirthday(DateTime date);
        bool CheckBirthday(int date);
        void BanMacs();
    }
}
