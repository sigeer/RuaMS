namespace Application.Core.Game.Commands.Gm5
{
    public class RunJsCommand : ParamsCommandBase
    {
        public RunJsCommand() : base(["<jsname>"], 5, "runjs")
        {
            Description = "调用Npc脚本";
        }

        public override Task Execute(IChannelClient client, string[] values)
        {
            client.OpenNpc(NpcId.Administrator, GetParam("jsname"));
            return Task.CompletedTask;
        }
    }
}
