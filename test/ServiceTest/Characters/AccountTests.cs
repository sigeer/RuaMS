using Application.Core.Managers;
using Application.EF;
using client.creator;
using client.creator.novice;
using Microsoft.EntityFrameworkCore;

namespace ServiceTest.Characters
{
    public class AccountTests
    {
        [Test]
        public void CreateAccount_Test()
        {
            var acc = AccountManager.CreateAccount("unittest", "unittest");
            Assert.That(acc > 0);
        }

        public void Create_Delete_Character_Test()
        {
            var r = CharacterFactory.CreateCharacter(0, 1, "abcdefg", 30032, 20000, 3, 0, BeginnerCreator.CreateRecipe(1040006, 1060006, 1072005, 1312004), out var newChar);
            Assert.That(r == 0 && newChar != null);

            if (r == 0 && newChar != null)
                Assert.That(CharacterManager.DeleteCharacterFromDB(newChar.getId()));
        }

        //[Test]
        //public void CharacterGMCheck_Test()
        //{
        //    using var dbContext = new DBContext();
        //    var query = dbContext.Characters.Where(x => x.Id == 19).Select(x => x.Gm);
        //    Console.WriteLine(query.ToQueryString());
        //    var c = query.FirstOrDefault();
        //    Assert.AreEqual(6, c);
        //}
    }
}
