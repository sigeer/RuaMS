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
            var newAccModel = new Account
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
    }
}
