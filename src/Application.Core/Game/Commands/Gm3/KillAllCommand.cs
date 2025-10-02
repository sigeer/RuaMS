using Application.Core.Managers;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class KillAllCommand : CommandBase
{
    public KillAllCommand() : base(3, "killall")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var count = AdminManager.KillAllMonster(c.OnlinedCharacter);
        c.OnlinedCharacter.MessageI18N(nameof(ClientMessage.KillAllCommand_MonsterKilled), count.ToString());
    }
}
