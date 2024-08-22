using System.Text;
using tools;

namespace ServiceTest.Infrastructure
{
    public class BCryptTests
    {
        [Test]
        public void HashTest()
        {
            var inputString = "admin";

            var r1 = BCrypt.hashpw(inputString, BCrypt.gensalt(12));
            var r2 = BCrypt.hashpw(inputString, BCrypt.gensalt(12));
            Console.WriteLine(r1);
            Assert.That(r1 != r2);
        }

        [Test]
        public void CheckTest()
        {
            var inputString = "admin";

            var r1 = BCrypt.hashpw(inputString, BCrypt.gensalt(12));
            Assert.That(BCrypt.checkpw(inputString, r1));
        }

    }
}
