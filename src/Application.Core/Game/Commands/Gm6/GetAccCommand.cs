namespace Application.Core.Game.Commands.Gm6;

public class GetAccCommand : CommandBase
{
    public GetAccCommand() : base(6, "getacc")
    {
        Description = "Show account name of an online player.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !getacc <playername>");
            return;
        }
        var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            await player.Pink(victim.getName() + "'s account name is " + victim.getClient().AccountEntity!.Name + ".");
        }
        else
        {
            await player.Pink("Player '" + paramsValue[0] + "' could not be found on this world.");
        }
    }
}
