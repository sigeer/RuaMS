using Application.Core.Managers;

namespace Application.Core.Game.Commands.Gm3;

public class KillAllCommand : CommandBase
{
    public KillAllCommand() : base(3, "killall")
    {
        Description = "Kill all mobs in the map.";
    }

    public override void Execute(IClient c, string[] paramsValue)
    {
        var count = AdminManager.KillAllMonster(c.OnlinedCharacter);
        c.OnlinedCharacter.dropMessage(5, "Killed " + count + " monsters.");
    }
}
