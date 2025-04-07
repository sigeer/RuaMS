using Application.Shared.Login;
using Microsoft.EntityFrameworkCore;
using net.server;
using net.server.coordinator.session;
using server;

namespace Application.Core.Game
{
    public class Account
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public IPlayer? OnlinedCharacter { get; }

        public List<IPlayer> AccountCharacterList { get; }

        public Storage Storage { get; }

        public string Pic { get; private set; }
        public string Pin { get; private set; }
        public string Mac { get; private set; }
        public DateTime BirthDay { get; private set; }
        public sbyte Gender { get; private set; }
        public int CharacterSlots { get; private set; }
        public DateTimeOffset? LastLogin { get; private set; }
        public int Language { get; private set; }
        public int LoginStage { get; private set; }

        readonly ILogger _logger;
        public IClient Client;
        public Account(IClient client)
        {
            Client = client;
            Id = -2;
            _logger = LogFactory.GetLogger(LogType.Login);

            AccountCharacterList = [];
        }

        public int GetLoginState()
        {
            if (LoginStage == AccountStage.LOGIN_SERVER_TRANSITION)
            {
                if (this.LastLogin!.Value.AddSeconds(30).ToUnixTimeMilliseconds() < Server.getInstance().getCurrentTime())
                {
                    UpdateLoginState(AccountStage.LOGIN_NOTLOGGEDIN);   // ACCID = 0, issue found thanks to Tochi & K u ssss o & Thora & Omo Oppa
                }
            }

            LoginStage = AccountStage.LOGIN_LOGGEDIN;
            return LoginStage;
        }

        public int GetLoginStateFromDB()
        {
            using var dbContext = new DBContext();
            var data = dbContext.Accounts.Where(x => x.Id == Id).Select(x => new { x.Loggedin, x.Lastlogin }).FirstOrDefault();
            if (data == null)
            {
                throw new Exception("Account不存在 " + Id);
            }

            LoginStage = data.Loggedin;
            LastLogin = data.Lastlogin;
            return GetLoginState();
        }

        public void UpdateLoginState(int newState)
        {
            // rules out possibility of multiple account entries
            if (newState == AccountStage.LOGIN_LOGGEDIN)
            {
                SessionCoordinator.getInstance().updateOnlineClient(Client);
            }

            using var dbContext = new DBContext();
            dbContext.Accounts.Where(x => x.Id == Id)
                .ExecuteUpdate(x => x.SetProperty(y => y.Loggedin, (sbyte)newState).SetProperty(y => y.Lastlogin, DateTimeOffset.Now));

            LoginStage = newState;
            LastLogin = DateTimeOffset.Now;
        }
    }
}
