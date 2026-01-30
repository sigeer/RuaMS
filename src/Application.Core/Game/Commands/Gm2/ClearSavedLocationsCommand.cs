namespace Application.Core.Game.Commands.Gm2;

public class ClearSavedLocationsCommand : CommandBase
{
    public ClearSavedLocationsCommand() : base(2, "clearsavelocs")
    {
        Description = "Clear saved locations for a player.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        Player? victim;

        if (paramsValue.Length > 0)
        {
            victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
            if (victim == null || !victim.IsOnlined)
            {
                player.message("Player '" + paramsValue[0] + "' could not be found.");
                return;
            }
        }
        else
        {
            victim = c.OnlinedCharacter;
        }

        foreach (SavedLocationType type in Enum.GetValues<SavedLocationType>())
        {
            victim.clearSavedLocation(type);
        }

        player.message("Cleared " + paramsValue[0] + "'s saved locations.");
    }
}
