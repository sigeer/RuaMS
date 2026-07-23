using Application.Core.Channel;
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
        void SetAccount(AccountInfoModel accountEntity);
        WorldChannel getChannelServer();
        int getChannel();


        bool CheckBirthday(DateTime date);
        bool CheckBirthday(int date);
        Task LeaveCashShop();
        Task ChangeChannel(int channel);
    }
}
