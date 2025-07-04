using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm4;

public class PnpcCommand : CommandBase
{
    readonly DataService _dataService;
    public PnpcCommand(DataService dataService) : base(4, "pnpc")
    {
        Description = "Spawn a permanent NPC on your location.";
        _dataService = dataService;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !pnpc <npcid>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var npcId))
        {
            player.yellowMessage("Syntax: npcid invalid");
            return;
        }

        // command suggestion thanks to HighKey21, none, bibiko94 (TAYAMO), asafgb
        if (player.getMap().containsNPC(npcId))
        {
            player.dropMessage(5, "This map already contains the specified NPC.");
            return;
        }

        _dataService.CreatePLife(player, npcId, LifeType.NPC);
    }
}
