using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Shared.Characters;
using Application.Shared.Dto;
using Application.Shared.Login;
using net.server.coordinator.session;

namespace Application.Core.Login.Client
{
    public interface ILoginClient : IClientBase
    {
        AccountDto? AccountEntity { get; protected set; }
        MasterServer CurrentServer { get; protected set; }
        int SelectedChannel { get; set; }
        void Disconnect();

        void updateLoginState(sbyte newState);
        bool CanBypassPin();
        bool CheckPin(string other);
        bool CanBypassPic();
        bool CheckPic(string other);

        bool isLoggedIn();
        LoginResultCode Login(string login, string pwd, Hwid nibbleHwid);
        LoginResultCode FinishLogin();
        bool CheckChar(int accid);
        bool HasBannedMac();
        bool HasBannedIP();
        bool HasBannedHWID();

        void UpdateMacs(string macData);

        void SendCharList();
        List<CharacterViewObject> LoadCharactersView();
        bool CanRequestCharlist();
        void UpdateRequestCharListTick();
        void CommitAccount();
    }
}
