using Application.Shared.Login;
using Microsoft.EntityFrameworkCore;
using net.server;
using net.server.coordinator.session;
using server;
using tools;
using static Mysqlx.Notice.Warning.Types;
using static net.server.coordinator.session.SessionCoordinator;

namespace Application.Core.Game
{
    public class Account : IAccount
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public IPlayer? OnlinedCharacter { get; }

        public List<IPlayer> AccountCharacterList { get; }

        public Storage Storage { get; }

        public string Pic { get; private set; }
        public string Pin { get; private set; }
        public string Mac { get; private set; }
        public DateTime? BirthDay { get; private set; }
        public sbyte Gender { get; private set; }
        public int CharacterSlots { get; private set; }
        public DateTimeOffset? LastLogin { get; private set; }
        public int Language { get; private set; }
        private int _loginState;
        bool _isLoggedIn;

        int loginCount;
        readonly ILogger _logger;
        string _inputAccountName;
        string _inputPassword;
        public IClient Client;
        public Account(IClient client, string accountName, string password)
        {
            Client = client;
            Id = -2;
            _inputAccountName = accountName;
            _inputPassword = password;
            _logger = LogFactory.GetLogger(LogType.Login);
        }

        public void Login()
        {

        }

        public void LoginSuccess()
        {
        }

        public LoginResultCode Login(Hwid nibbleHwid)
        {
            LoginResultCode loginResult = LoginResultCode.Success;

            try
            {
                using var dbContext = new DBContext();
                var dbModel = dbContext.Accounts.Where(x => x.Name == _inputAccountName).FirstOrDefault();
                if (dbModel == null)
                {
                    Id = -3;
                    return LoginResultCode.Fail_AccountNotExsited;
                }

                if (dbModel.Banned == 1)
                {
                    return LoginResultCode.Fail_Banned;
                }

                Id = dbModel.Id;
                if (Id <= 0)
                {
                    _logger.Error("Tried to login with accid {AccountId}", Id);
                    return LoginResultCode.Fail_SpecialAccount;
                }

                Name = dbModel.Name;
                Pin = dbModel.Pin;
                Pic = dbModel.Pic;
                Gender = dbModel.Gender;
                CharacterSlots = dbModel.Characterslots;
                BirthDay = dbModel.Birthday;
                Language = dbModel.Language;
                string passhash = dbModel.Password;
                var tos = dbModel.Tos;

                _loginState = dbModel.Loggedin;
                LastLogin = dbModel.Lastlogin;

                if (GetLoginState() > AccountStage.LOGIN_NOTLOGGEDIN)
                {
                    // already loggedin
                    _isLoggedIn = false;
                    return LoginResultCode.Fail_AlreadyLoggedIn;
                }
                else if (passhash.ElementAt(0) == '$' && passhash.ElementAt(1) == '2' && BCrypt.checkpw(_inputPassword, passhash))
                {
                    return !tos ? LoginResultCode.Fail_Error23 : LoginResultCode.Success;
                }
                else if (_inputPassword.Equals(passhash)
                    || HashDigest.HashByType("SHA-1", _inputPassword).ToHexString().Equals(passhash)
                    || HashDigest.HashByType("SHA-512", _inputPassword).ToHexString().Equals(passhash))
                {
                    // thanks GabrielSin for detecting some no-bcrypt inconsistencies here
                    return !tos
                        ? (!YamlConfig.config.server.BCRYPT_MIGRATION ? LoginResultCode.Fail_Error23 : LoginResultCode.CheckTOS)
                        : (!YamlConfig.config.server.BCRYPT_MIGRATION ? LoginResultCode.Success : LoginResultCode.MigrateBCrypto); // migrate to bcrypt
                }
                else
                {
                    _isLoggedIn = false;
                    return LoginResultCode.Fail_IncorrectPassword;
                }
            }
            catch (DbUpdateException e)
            {
                _logger.Error(e.ToString());
                return LoginResultCode.Fail_AccountNotExsited;
            }
        }

        public int GetLoginState()
        {
            try
            {
                if (_loginState == AccountStage.LOGIN_SERVER_TRANSITION)
                {
                    if (this.LastLogin!.Value.AddSeconds(30).ToUnixTimeMilliseconds() < Server.getInstance().getCurrentTime())
                    {
                        UpdateLoginState(AccountStage.LOGIN_NOTLOGGEDIN);   // ACCID = 0, issue found thanks to Tochi & K u ssss o & Thora & Omo Oppa
                    }
                }
                if (_loginState == AccountStage.LOGIN_LOGGEDIN)
                {
                    _isLoggedIn = true;
                }
                else if (_loginState == AccountStage.LOGIN_SERVER_TRANSITION)
                {
                    _loginState = AccountStage.LOGIN_NOTLOGGEDIN;
                }
                else
                {
                    _isLoggedIn = false;
                }

                return _loginState;
            }
            catch (DbUpdateException e)
            {
                _isLoggedIn = false;
                _logger.Error(e.ToString());
                throw new Exception("login state");
            }
        }

        public void UpdateLoginState(int newState)
        {
            // rules out possibility of multiple account entries
            if (newState == AccountStage.LOGIN_LOGGEDIN)
            {
                SessionCoordinator.getInstance().updateOnlineClient(Client);
            }

            if (newState == AccountStage.LOGIN_NOTLOGGEDIN)
            {
                _isLoggedIn = false;
                // serverTransition = false;
                Id = 0;
            }
            else
            {
                // serverTransition = (newState == AccountStage.LOGIN_SERVER_TRANSITION);
                //_isLoggedIn = !serverTransition;
            }
            _loginState = newState;
            LastLogin = DateTimeOffset.Now;
        }
    }
}
