using Application.Core.Compatible.Extensions;
using Application.Core.Game;
using Application.Core.Game.Commands;

namespace ServiceTest.Games
{
    public class CommandTests : TestBase
    {
        [Test]
        public void GetGmCommandsTest()
        {
            var data = CommandExecutor.getInstance().getGmCommands();
            Assert.That(data.Size(), Is.EqualTo(7));
        }

        [Test]
        public void ParamsCommandTest()
        {
            var testCommand = new TestParamsCommand();
            Assert.That(testCommand.CurrentCommand == "test");
            Assert.That(testCommand.ValidSytax == "!test <a|b|c> <1|2|3>");
            testCommand.CurrentCommand = "demo";
            Assert.That(testCommand.ValidSytax == "!demo <a|b|c> <1|2|3>");
        }
    }

    public class TestParamsCommand : ParamsCommandBase
    {
        public TestParamsCommand() : base([["a", "b", "c"],["1", "2", "3"]], 0, "test", "demo")
        {
        }

        public override void Execute(IClient client, string[] values)
        {
            throw new NotImplementedException();
        }
    }
}
