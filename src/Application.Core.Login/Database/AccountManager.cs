using Application.Core.tools;
using Application.EF;
using Application.EF.Entities;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Login.Database
{
    public class AccountManager
    {
        readonly ILogger<AccountManager> log;

        public AccountManager(ILogger<AccountManager> log)
        {
            this.log = log;
        }

        public int CreateAccount(string loginAccount, string pwd)
        {
            using var dbContext = new DBContext();
            var password = HashDigest.HashByType("SHA-512", pwd).ToHexString();
            var newAccModel = new AccountEntity(loginAccount, password);
            dbContext.Accounts.Add(newAccModel);
            dbContext.SaveChanges();
            return newAccModel.Id;
        }

        public bool IsAccountHasCharacter(int accId, int charId)
        {
            using var dbContext = new DBContext();
            return dbContext.Characters.Any(x => x.AccountId == accId && x.Id == charId);
        }
    }
}
