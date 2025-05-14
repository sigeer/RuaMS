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

    }
}
