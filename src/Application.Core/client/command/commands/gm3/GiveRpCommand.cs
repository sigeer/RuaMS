namespace client.command.commands.gm3;

public class GiveRpCommand : Command
{
    public GiveRpCommand()
    {
        setDescription("Give reward points to a player.");
    }

    public override void execute(Client client, string[] paramsValue)
    {
        Character player = client.getPlayer();
        if (paramsValue.Length < 2)
        {
            player.yellowMessage("Syntax: !giverp <playername> <gainrewardpoint>");
            return;
        }

        var victim = client.getWorldServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null)
        {
            victim.setRewardPoints(victim.getRewardPoints() + int.Parse(paramsValue[1]));
            player.message("RP given. Player " + paramsValue[0] + " now has " + victim.getRewardPoints()
                    + " reward points.");
        }
        else
        {
            player.message("Player '" + paramsValue[0] + "' could not be found.");
        }
    }
}
