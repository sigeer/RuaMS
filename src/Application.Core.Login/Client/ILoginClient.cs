using Application.Core.Login.Models;
using Application.Shared.Login;

namespace Application.Core.Login.Client
{
    public interface ILoginClient : IClientBase
    {
        new AccountCtrl? AccountEntity { get; set; }
        MasterServer CurrentServer { get; protected set; }
        int SelectedChannel { get; set; }
        int CurrentHistoryId { get; set; }
        Task Disconnect();

        Task updateLoginState(sbyte newState);
        bool CanBypassPin();
        bool CheckPin(string other);
        bool CanBypassPic();
        bool CheckPic(string other);

        bool isLoggedIn();
        Task<LoginResultCode> Login(string login, string pwd, Hwid nibbleHwid);
        Task<LoginResultCode> FinishLogin();
        bool CheckChar(int accid);
        Task SendCharList();
        List<CharacterLiveObject> LoadCharactersView();
        bool CanRequestCharlist();
        void UpdateRequestCharListTick();
        bool AcceptToS();
    }
}
