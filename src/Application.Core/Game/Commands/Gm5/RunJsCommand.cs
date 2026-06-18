namespace Application.Core.Game.Commands.Gm5
{
    public class RunJsCommand : ParamsCommandBase
    {
        public RunJsCommand() : base(["<jsname>"], 5, "runjs")
        {
            Description = "调用Npc脚本";
        }

        public override async Task Execute(IChannelClient client, string[] values)
        {
            await client.OnlinedCharacter.OpenNpc(NpcId.Administrator, GetParam("jsname"));
        }
    }
}
