using server.life;

namespace Application.Core.Game.Commands.Gm4;

public class PapCommand : CommandBase
{
    public PapCommand() : base(4, "pap")
    {
        Description = "Spawn Papulatus on your location.";
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        // thanks Conrad for noticing mobid typo here
        player.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(MobId.PAPULATUS_CLOCK), player.getPosition());
        return Task.CompletedTask;
    }
}
