using Application.Core.Compatible.Extensions;
using Application.Core.Game;
using Application.Core.Game.Commands;
using Application.Scripting.JS;

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
        public void ParamsCommand1Test()
        {
            var testCommand = new TestParamsCommand1();
            Assert.That(testCommand.CurrentCommand == "test");
            Assert.That(testCommand.ValidSytax, Is.EqualTo("!test [a|b|c] [1|2|3]"));
            Assert.That(testCommand.CheckArguments(["a", "3"]));
            Assert.That(!testCommand.CheckArguments(["a", "4"]));
            Assert.That(!testCommand.CheckArguments(["A", "3"]));
            testCommand.CurrentCommand = "demo";
            Assert.That(testCommand.ValidSytax, Is.EqualTo("!demo [a|b|c] [1|2|3]"));
        }

        [Test]
        public void ParamsCommand2Test()
        {
            var testCommand = new TestParamsCommand2();
            Assert.That(testCommand.CurrentCommand == "test");
            Assert.That(testCommand.ValidSytax, Is.EqualTo("!test [a|b|c] <id>"));
            Assert.That(testCommand.CheckArguments(["a", "4"]));
            Assert.That(testCommand.CheckArguments(["a", "4"]));
            Assert.That(!testCommand.CheckArguments(["A", "3"]));
            testCommand.CurrentCommand = "demo";
            Assert.That(testCommand.ValidSytax, Is.EqualTo("!demo [a|b|c] <id>"));
            testCommand.Run(MockClient, ["a", "123"]);
        }
    }

    public class TestParamsCommand1 : ParamsCommandBase
    {
        public TestParamsCommand1() : base(["[a|b|c]", "[1|2|3]"], 0, "test", "demo")
        {
        }

        public override void Execute(IClient client, string[] values)
        {
            throw new NotImplementedException();
        }
    }

    public class TestParamsCommand2 : ParamsCommandBase
    {
        public TestParamsCommand2() : base(["[a|b|c]", "<id>"], 0, "test", "demo")
        {
        }

        public override void Execute(IClient client, string[] values)
        {
            var id = GetIntParam("id");
            Assert.That(id, Is.EqualTo(123));
        }
    }
}
