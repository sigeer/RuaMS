using Application.Core.Managers;
using client.creator;
using client.creator.novice;

namespace ServiceTest.Characters
{
    public class AccountTests: TestBase
    {
        //[Test]
        //public void Create_DeleteAccount_Test()
        //{
        //    var acc = AccountManager.CreateAccount("test001", "test001");
        //    Assert.That(acc > 0);
        //}

        [Test]
        public void Create_Delete_Character_Test()
        {
            var r = CharacterFactory.CreateCharacter(0, 1, "abcdefg", 20000, 30032, 3, 0, BeginnerCreator.CreateRecipe(1040006, 1060006, 1072005, 1312004), out var newChar);
            Assert.That(r, Is.EqualTo(0));

            if (r == 0 && newChar != null)
                Assert.That(CharacterManager.DeleteCharacterFromDB(newChar.getId()));
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
            return CharacterManager.CheckCharacterName(name);
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
