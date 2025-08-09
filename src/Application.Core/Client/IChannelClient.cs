using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Scripting.Infrastructure;
using scripting;
using scripting.Event;
using scripting.npc;

namespace Application.Core.Client
{
    public interface IChannelClient : IClientBase
    {
        WorldChannel CurrentServer { get; }
        WorldChannelServer CurrentServerContainer => CurrentServer.Container;
        int Channel { get; }
        IPlayer? Character { get; }
        IPlayer OnlinedCharacter { get; }

        NPCConversationManager? NPCConversationManager { get; set; }
        EngineStorage ScriptEngines { get; set; }

        void Disconnect(bool isShutdown, bool fromCashShop = false);

        void enableCSActions();
        AbstractPlayerInteraction getAbstractPlayerInteraction();
        void lockClient();
        void unlockClient();
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
        WorldChannel getChannelServer();
        int getChannel();
        void announceBossHpBar(Monster mm, int mobHash, Packet packet);

        EventManager? getEventManager(string @event);
        void removeClickedNPC();

        bool CheckBirthday(DateTime date);
        bool CheckBirthday(int date);
    }
}
