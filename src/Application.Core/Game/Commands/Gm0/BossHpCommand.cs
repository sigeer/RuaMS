using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm0;

public class BossHpCommand : CommandBase
{
    public BossHpCommand() : base(0, "bosshp")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        player.getMap().ProcessMonster(monster =>
        {
            if (monster != null && monster.isBoss() && monster.getHp() > 0)
            {
                long percent = monster.getHp() * 100L / monster.getMaxHp();
                string bar = "[";
                for (int i = 0; i < 100; i++)
                {
                    bar += i < percent ? "|" : ".";
                }
                bar += "]";
                player.YellowMessageI18N(nameof(ClientMessage.BossHpCommand_Message1), c.CurrentCulture.GetMobName(monster.getId()), monster.getId().ToString(), percent.ToString());
                player.YellowMessageI18N(nameof(ClientMessage.BossHpCommand_Message2), bar);
            }
        });
        return Task.CompletedTask;
    }
}
