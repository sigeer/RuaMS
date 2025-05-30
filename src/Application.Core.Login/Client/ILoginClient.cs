using Application.Core.Login.Models;
using Application.Shared.Login;

namespace Application.Core.Login.Client
{
    public interface ILoginClient : IClientBase
    {
        AccountCtrl? AccountEntity { get; protected set; }
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
