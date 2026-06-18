using server.life;

namespace Application.Core.Game.Commands.Gm4;

public class PinkbeanCommand : CommandBase
{
    public PinkbeanCommand() : base(4, "pinkbean")
    {
        Description = "Spawn Pink Bean on your location.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        await player.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(MobId.PINK_BEAN), player.getPosition());
    }
}
