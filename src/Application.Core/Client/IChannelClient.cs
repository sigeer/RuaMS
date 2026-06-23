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

        bool GainCharacterSlot();
        void SetPlayer(Player? player);
        void SetAccount(AccountCtrl accountEntity);
        WorldChannel getChannelServer();
        int getChannel();


        bool CheckBirthday(DateTime date);
        bool CheckBirthday(int date);
        Task LeaveCashShop();
        Task ChangeChannel(int channel);
    }
}
