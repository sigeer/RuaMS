using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm4;

public class PnpcRemoveCommand : CommandBase
{
    readonly DataService _dataService;
    public PnpcRemoveCommand(DataService dataService) : base(4, "pnpcremove")
    {
        Description = "Remove a permanent NPC on the map.";
        _dataService = dataService;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        int npcId = paramsValue.Length > 0 ? int.Parse(paramsValue[0]) : -1;

       await  _dataService.RemovePLife(player, LifeType.NPC);
    }
}
