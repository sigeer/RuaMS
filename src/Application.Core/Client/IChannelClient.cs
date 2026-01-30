using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Scripting.Events;
using Application.Core.Scripting.Infrastructure;
using scripting;
using scripting.Event;
using scripting.npc;

namespace Application.Core.Client
{
    public interface IChannelClient : IClientBase
    {
        WorldChannel CurrentServer { get; }
        int Channel { get; }
        Player? Character { get; }
        Player OnlinedCharacter { get; }
        ClientCulture CurrentCulture { get; set; }

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
        bool GainCharacterSlot();

        bool attemptCsCoupon();
        void resetCsCoupon();
        void SetPlayer(Player? player);
        void SetAccount(AccountCtrl accountEntity);
        WorldChannel getChannelServer();
        int getChannel();
        void announceBossHpBar(Monster mm, int mobHash, Packet packet);

        EventManager? getEventManager(string @event);
        void removeClickedNPC();

        bool CheckBirthday(DateTime date);
        bool CheckBirthday(int date);
        void LeaveCashShop();
        void ChangeChannel(int channel);
    }
}
