using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Scripting.Events;
using scripting;
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
        Task Disconnect(bool isShutdown, bool fromCashShop = false);

        Task enableCSActions();
        AbstractPlayerInteraction getAbstractPlayerInteraction();
        bool canClickNPC();
        void setClickedNPC();

        Task closePlayerScriptInteractions();

        bool GainCharacterSlot();

        bool attemptCsCoupon();
        void resetCsCoupon();
        void SetPlayer(Player? player);
        void SetAccount(AccountCtrl accountEntity);
        WorldChannel getChannelServer();
        int getChannel();


        AbstractEventManager? getEventManager(string @event);
        void removeClickedNPC();

        bool CheckBirthday(DateTime date);
        bool CheckBirthday(int date);
        Task LeaveCashShop();
        Task ChangeChannel(int channel);
        #region Announce
        Task announceServerMessage();
        Task announceBossHpBar(Monster mm, int mobHash, Packet packet);
        Task announceHint(string msg, int length);
        #endregion
    }
}
