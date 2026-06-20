using Application.Core.Managers;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class KillAllCommand : CommandBase
{
    public KillAllCommand() : base(3, "killall")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var count = await AdminManager.KillAllMonster(c.OnlinedCharacter);
        await c.OnlinedCharacter.Pink(nameof(ClientMessage.KillAllCommand_MonsterKilled), count.ToString());
    }
}
