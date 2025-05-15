using Application.Core.Game.Life;
using Application.Core.Game.TheWorld;
using Application.Core.Scripting.Infrastructure;
using client.inventory;
using net.packet;
using scripting;
using scripting.Event;
using scripting.npc;

namespace Application.Core.Client
{
    public interface IChannelClient : IClientBase
    {
        new IWorldChannel CurrentServer { get; }
        int Channel { get; }
        int ActualChannel { get; }
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
        void SetAccount(AccountEntity? accountEntity);
        IWorldChannel getChannelServer();
        int getChannel();
        void announceBossHpBar(Monster mm, int mobHash, Packet packet);

        EventManager? getEventManager(string @event);
        void removeClickedNPC();
    }
}
