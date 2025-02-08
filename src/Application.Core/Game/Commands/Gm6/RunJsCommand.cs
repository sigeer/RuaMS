using constants.id;
using scripting.npc;

namespace Application.Core.Game.Commands.Gm6
{
    public class RunJsCommand : ParamsCommandBase
    {
        public RunJsCommand() : base(["<jsname>"], 6, "runjs")
        {
            Description = "调用Npc脚本";
        }

        public override void Execute(IClient client, string[] values)
        {
            client.OpenNpc(NpcId.Administrator, GetParam("jsname"));
        }
    }
}
