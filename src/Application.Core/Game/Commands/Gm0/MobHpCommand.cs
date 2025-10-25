namespace Application.Core.Game.Commands.Gm0;
public class MobHpCommand : CommandBase
{
    public MobHpCommand() : base(0, "mobhp")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        player.getMap().ProcessMonster(monster =>
        {
            if (monster != null && monster.getHp() > 0)
            {
                player.yellowMessage(c.CurrentCulture.GetMobName(monster.getId()) + " (" + monster.getId() + ") has " + monster.getHp() + " / " + monster.getMaxHp() + " HP.");
            }
        });
    }
}
