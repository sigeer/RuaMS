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

            var eim = chr.getEventInstance();
            if (!chr.isLeader() || eim == null)
            {
                // 只有队长在副本中才能使用
                await chr.Pink("只有队长在副本中才能使用");
                return;
            }

            var operation = GetParamByIndex(0);
            var idxStr = GetParam("idx");
            if (operation == "summon")
            {
                if (int.TryParse(idxStr, out var idx))
                {
                    if (idx <= 0 || idx > 5)
                    {
                        await chr.Pink("指令格式：!fake [summon|remove] [1|2|3|4|5]");
                        return;
                    }
                    await _manager.Summon(chr, idx);
                }
                else
                {
                    await chr.Pink("指令格式：!fake [summon|remove] [1|2|3|4|5]");
                    return;
                }

            }
            else if (operation == "remove")
            {
                if (int.TryParse(idxStr, out var idx))
                {
                    await _manager.Remove(eim, idx);
                }
            }


        }
    }
}
