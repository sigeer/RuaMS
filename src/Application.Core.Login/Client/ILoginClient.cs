using Application.Core.Login.Models;
using Application.Shared.Login;

namespace Application.Core.Login.Client
{
    public interface ILoginClient : IClientBase
    {
        MasterServer CurrentServer { get; protected set; }
        int SelectedChannel { get; set; }
        Task Disconnect();

        void updateLoginState(sbyte newState);
        bool CanBypassPin();
        Task<bool> CheckPin(string other);
        bool CanBypassPic();
        Task<bool> CheckPic(string other);

        bool isLoggedIn();
        Task<LoginResultCode> Login(string login, string pwd, Hwid nibbleHwid);
        LoginResultCode FinishLogin();
        Task<bool> CheckChar(int accid);
        void SendCharList();
        List<CharacterViewObject> LoadCharactersView();
        bool CanRequestCharlist();
        void UpdateRequestCharListTick();
        bool AcceptToS();
    }
}
