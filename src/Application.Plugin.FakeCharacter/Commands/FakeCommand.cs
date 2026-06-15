using Application.Core.Client;
using Application.Core.Game.Commands;

namespace Application.Plugin.FakeCharacter.Commands
{
    internal class FakeCommand : ParamsCommandBase
    {
        FakerService _manager;
        public FakeCommand(FakerService manager) : base(["[summon|remove]", "<idx>"], 0, "fake")
        {
            Description = "召唤一名假人，只能由队长召唤。切换频道后消失";
            _manager = manager;
        }

        public override void Execute(IChannelClient client, string[] values)
        {
            var chr = client.OnlinedCharacter;

            if (!chr.isLeader())
            {
                // 只有队长才能使用
                return;
            }

            var operation = GetParamByIndex(0);
            var idxStr = GetParam("idx");
            if (operation == "summon")
            {

                if (idxStr.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    // 
                }
                else if (int.TryParse(idxStr, out var idx))
                {
                    var fakeChr = _manager.GetOrCreateFakePlayer(chr, idx);
                    chr.MapModel.addPlayer(fakeChr);
                }

            }
            else if (operation == "remove")
            {
                if (idxStr.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    // 
                }
                else if (int.TryParse(idxStr, out var idx))
                {
                    _manager.Remove(chr, idx);
                }
            }


        }
    }
}
