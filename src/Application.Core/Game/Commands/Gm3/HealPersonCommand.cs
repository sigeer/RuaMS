namespace Application.Core.Game.Commands.Gm3;

public class HealPersonCommand : CommandBase
{
    public HealPersonCommand() : base(3, "healperson")
    {
        Description = "Heal all HP/MP of a player.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            await victim.healHpMp();
        }
        else
        {
            await player.Pink("Player '" + paramsValue[0] + "' could not be found on this channel.");
        }
    }
}
