using Application.Core.Servers;
using Application.Shared.Login;
using net.server.coordinator.session;

namespace Application.Core.Client
{
    public interface ILoginClient : IClientBase
    {
        new IMasterServer CurrentServer { get; protected set; }
        int SelectedChannel { get; set; }
        void Disconnect();

        void updateLoginState(sbyte newState);
        bool CanBypassPin();
        bool CheckPin(string other);
        bool CanBypassPic();
        bool CheckPic(string other);

        bool isLoggedIn();

        int GetLoginState();
        LoginResultCode Login(string login, string pwd, Hwid nibbleHwid);
        LoginResultCode FinishLogin();
        bool CheckChar(int accid);
        bool HasBannedMac();
        bool HasBannedIP();
        bool HasBannedHWID();

        void UpdateMacs(string macData);

        void CommitAccount();

        void SetCharacterOnSessionTransitionState(int cid);
        void SendCharList();
        List<IPlayer> LoadCharacters();

    }
}
