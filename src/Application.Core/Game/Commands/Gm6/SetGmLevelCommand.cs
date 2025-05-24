namespace Application.Core.Game.Commands.Gm6;

public class SetGmLevelCommand : CommandBase
{
    public SetGmLevelCommand() : base(6, "setgmlevel")
    {
        Description = "Set GM level of a player.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            player.yellowMessage("Syntax: !setgmlevel <playername> <newlevel>");
            return;
        }

        int newLevel = int.Parse(paramsValue[1]);
        var target = c.CurrentServer.getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (target != null && target.Client.AccountEntity != null)
        {
            target.Client.AccountEntity.GMLevel = (sbyte)newLevel;

            target.dropMessage("You are now a level " + newLevel + " GM. See @commands for a list of available commands.");
            player.dropMessage(target + " is now a level " + newLevel + " GM.");
        }
        else
        {
            player.dropMessage("Player '" + paramsValue[0] + "' was not found on this channel.");
        }
    }
}
