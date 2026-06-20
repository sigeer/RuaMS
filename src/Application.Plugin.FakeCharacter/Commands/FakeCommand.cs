using Application.Core.Client;
using Application.Core.Game.Commands;

namespace Application.Plugin.FakeCharacter.Commands
{
    internal class FakeCommand : ParamsCommandBase
    {
        FakerService _manager;
        public FakeCommand(FakerService manager) : base(["[summon|remove]", "<idx>"], 0, "fake")
        {
            Description = "召唤一名假人。只能由队长在副本地图召唤。";
            _manager = manager;
        }

        public override async Task Execute(IChannelClient client, string[] values)
        {
            var chr = client.OnlinedCharacter;

            //if (!chr.isLeader() || chr.getEventInstance() != null)
            //{
            //    // 只有队长在副本中才能使用
            //    return;
            //}

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
                    await _manager.Summon(chr, idx);
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
                    await _manager.Remove(chr, idx);
                }
            }


        }
    }
}
