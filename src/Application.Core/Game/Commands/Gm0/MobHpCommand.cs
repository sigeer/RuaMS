namespace Application.Core.Game.Commands.Gm0;

public class MobHpCommand : CommandBase
{
    public MobHpCommand() : base(0, "mobhp")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        await player.getMap().ProcessMonster(async monster =>
         {
             if (monster != null && monster.getHp() > 0)
             {
                 await player.Yellow(c.CurrentCulture.GetMobName(monster.getId()) + " (" + monster.getId() + ") has " + monster.getHp() + " / " + monster.getMaxHp() + " HP.");
             }
         });
    }
}
