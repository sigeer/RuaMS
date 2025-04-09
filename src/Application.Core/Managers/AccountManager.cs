using Application.Shared.Login;
using Microsoft.EntityFrameworkCore;
using net.server.coordinator.session;
using tools;
using static net.server.coordinator.session.SessionCoordinator;

namespace Application.Core.Managers
{
    public class AccountManager
    {
        static ILogger _logger = LogFactory.GetLogger(LogType.Login);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginAccount"></param>
        /// <param name="pwd"></param>
        /// <returns>AccountId</returns>
        public static int CreateAccount(string loginAccount, string pwd)
        {
            using var dbContext = new DBContext();
            var password = HashDigest.HashByType("SHA-512", pwd).ToHexString();
            var newAccModel = new AccountEntity(loginAccount, password);
            dbContext.Accounts.Add(newAccModel);
            dbContext.SaveChanges();
            return newAccModel.Id;
        }

        /// <summary>
        /// 获取account 在 world的 角色id
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="world"> -1时获取全部， 默认-1</param>
        /// <returns></returns>
        public static List<int> LoadAccountWorldPlayers(int accountId)
        {
            using var dbContext = new DBContext();
            return dbContext.Characters.Where(x => x.AccountId == accountId).OrderBy(x => x.Id).Select(x => x.Id).ToList();
        }

        public static List<int> LoadAccountWorldPlayers(int accountId, IEnumerable<int> worlds)
        {
            using var dbContext = new DBContext();
            return dbContext.Characters.Where(x => x.AccountId == accountId && worlds.Contains(x.World)).OrderBy(x => x.World).ThenBy(x => x.Id).Select(x => x.Id).ToList();
        }

        public static LoginResultCode ClientLogin(IClient client, string accountName, string password, Hwid nibbleHwid, out Account? account)
        {
            account = null;

            //client.LoginAttemptCount++;
            //if (client.LoginAttemptCount > 4)
            //{
            //    SessionCoordinator.getInstance().closeSession(client, false);
            //    return LoginResultCode.Fail_Count;
            //}

            LoginResultCode loginResultCode = LoginResultCode.Fail_AccountNotExsited;
            try
            {
                using var dbContext = new DBContext();
                var dbModel = dbContext.Accounts.Where(x => x.Name == accountName).FirstOrDefault();
                if (dbModel == null)
                {
                    return LoginResultCode.Fail_AccountNotExsited;
                }

                account = GlobalTools.Mapper.Map<Account>(dbModel, cfg => cfg.ConstructServicesUsing(x => new Account(client)));
                if (dbModel.Banned == 1)
                {
                    return LoginResultCode.Fail_Banned;
                }

                if (account.Id <= 0)
                {
                    _logger.Error("Tried to login with accid {AccountId}", account.Id);
                    return LoginResultCode.Fail_SpecialAccount;
                }

                if (account.GetLoginState() > AccountStage.LOGIN_NOTLOGGEDIN)
                {
                    // already loggedin
                    return LoginResultCode.Fail_AlreadyLoggedIn;
                }
                else if (password.Equals(dbModel.Password)
                    || HashDigest.HashByType("SHA-512", password).ToHexString().Equals(dbModel.Password))
                {
                    loginResultCode = !dbModel.Tos
                        ? LoginResultCode.Fail_Agreement
                        : LoginResultCode.Success;
                }
                else
                {
                    loginResultCode = LoginResultCode.Fail_IncorrectPassword;
                }


                if (loginResultCode == LoginResultCode.Success || loginResultCode == LoginResultCode.Fail_IncorrectPassword)
                {
                    AntiMulticlientResult res = SessionCoordinator.getInstance().attemptLoginSession(client, nibbleHwid, dbModel.Id, loginResultCode == LoginResultCode.Success);

                    switch (res)
                    {
                        case AntiMulticlientResult.SUCCESS:
                            if (loginResultCode == LoginResultCode.Success)
                            {
                                // client.LoginAttemptCount = 0;
                            }
                            break;
                        case AntiMulticlientResult.REMOTE_LOGGEDIN:
                            return LoginResultCode.Fail_Error17;

                        case AntiMulticlientResult.REMOTE_REACHED_LIMIT:
                            return LoginResultCode.Fail_Error13;

                        case AntiMulticlientResult.REMOTE_PROCESSING:
                            return LoginResultCode.Fail_Error10;

                        case AntiMulticlientResult.MANY_ACCOUNT_ATTEMPTS:
                            return LoginResultCode.Fail_Error16;

                        default:
                            return LoginResultCode.Fail_Error8;
                    }
                }
                return loginResultCode;
            }
            catch (DbUpdateException e)
            {
                _logger.Error(e.ToString());
                return LoginResultCode.Fail_AccountNotExsited;
            }


        }

    }
}
