using Microsoft.EntityFrameworkCore;
using tools;

namespace Application.Core.Managers
{
    public class AccountManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginAccount"></param>
        /// <param name="pwd"></param>
        /// <returns>AccountId</returns>
        public static int CreateAccount(string loginAccount, string pwd)
        {
            using var dbContext = new DBContext();
            var password = YamlConfig.config.server.BCRYPT_MIGRATION ? BCrypt.hashpw(pwd, BCrypt.gensalt(12)) : HashDigest.HashByType("SHA-512", pwd).ToHexString();
            var newAccModel = new AccountEntity
            {
                Name = loginAccount,
                Password = password,
                Birthday = DateTime.Now.Date
            };
            dbContext.Accounts.Add(newAccModel);
            dbContext.SaveChanges();
            return newAccModel.Id;
        }

        public static void UpdatePasswordToBCrypt(string loginAccount, string pwd)
        {
            using var dbContext = new DBContext();
            dbContext.Accounts.Where(x => x.Name == loginAccount).ExecuteUpdate(x => x.SetProperty(y => y.Password, BCrypt.hashpw(pwd, BCrypt.gensalt(12))));
        }

        public static List<int> LoadAccountWorldPlayers(int accountId, int world)
        {
            using var dbContext = new DBContext();
            return dbContext.Characters.Where(x => x.AccountId == accountId && x.World == world).OrderBy(x => x.Id).Select(x => x.Id).ToList();
        }

    }
}
