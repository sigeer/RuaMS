using Application.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ServiceTest.Games.Account
{
    public class AccountTests
    {
        //[Test]
        //public void Create_DeleteAccount_Test()
        //{
        //    var acc = AccountManager.CreateAccount("test001", "test001");
        //    Assert.That(acc > 0);
        //}

        [Test]
        public void BaseTest()
        {
            var p = GameTestGlobal.TestServer.GetPlayer();
            Assert.That(p is not null);
        }

        [Test]
        public async Task Create_Delete_Character_Test()
        {
            var acc = GameTestGlobal.TestServer.GetMasterServer().AccountManager.GetAccountDto(1)!;

            var r = GameTestGlobal.TestServer.GetMasterServer().CreatePlayerService.CreateCharacter(acc, 1, "abcdefg", 0, 20000, 30032, 3, 1040002, 1060002, 1072001, 1302000);
            Assert.That(r, Is.Not.Null);

            using var dbContext = GameTestGlobal.TestServer.ServiceProvider.GetRequiredService<IDbContextFactory<DBContext>>().CreateDbContext();
            using (var dbTran = dbContext.Database.BeginTransaction())
            {
                await GameTestGlobal.TestServer.GetMasterServer().CharacterManager.Commit(dbContext);
                dbTran.Commit();
            }

            var player = GameTestGlobal.TestServer.GetPlayer(r.Character.Id);
            Assert.That(player, Is.Not.Null);

            Assert.That(GameTestGlobal.TestServer.GetMasterServer().CharacterManager.RemoveCharacter(r.Character.Id, r.Character.AccountId));
            using (var dbTran = dbContext.Database.BeginTransaction())
            {
                await GameTestGlobal.TestServer.GetMasterServer().CharacterManager.Commit(dbContext);
                dbTran.Commit();
            }

            player = GameTestGlobal.TestServer.GetPlayer(r.Character.Id);
            Assert.That(player, Is.Null);
        }


        [TestCase("   ", ExpectedResult = false)]
        [TestCase("1   2", ExpectedResult = false)]
        [TestCase("12", ExpectedResult = false)]
        [TestCase("12??", ExpectedResult = false)]
        [TestCase("1234567", ExpectedResult = true)]
        [TestCase("1234567??", ExpectedResult = false)]
        [TestCase("12345671234567", ExpectedResult = false)]
        [TestCase("qw", ExpectedResult = false)]
        [TestCase("qwertyu", ExpectedResult = true)]
        [TestCase("qwertyuqwertyu", ExpectedResult = false)]
        [TestCase("张三", ExpectedResult = true)]
        [TestCase("张三李四", ExpectedResult = true)]
        [TestCase("张三李四1", ExpectedResult = true)]
        [TestCase("张三李四王五", ExpectedResult = true)]
        [TestCase("张三李四王五1", ExpectedResult = false)]
        [TestCase("张三李4", ExpectedResult = true)]
        [TestCase("张三李45", ExpectedResult = true)]
        [TestCase("哟", ExpectedResult = false)]
        [TestCase("😄😄", ExpectedResult = false)]
        [Test]
        public bool CheckCharacterName_Test(string name)
        {
            return GameTestGlobal.TestServer.GetMasterServer().CharacterManager.CheckCharacterName(name);
        }
    }
}
