namespace Application.Core.Game.Commands.Gm2;

public class ClearSavedLocationsCommand : CommandBase
{
    public ClearSavedLocationsCommand() : base(2, "clearsavelocs")
    {
        Description = "Clear saved locations for a player.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        Player? victim;

        if (paramsValue.Length > 0)
        {
            victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
            if (victim == null || !victim.IsOnlined)
            {
                await player.Pink("Player '" + paramsValue[0] + "' could not be found.");
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

        await player.Pink("Cleared " + paramsValue[0] + "'s saved locations.");
    }
}
