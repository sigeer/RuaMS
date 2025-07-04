using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm4;

public class PmobRemoveCommand : CommandBase
{
    readonly DataService _dataService;
    public PmobRemoveCommand(DataService dataService) : base(4, "pmobremove")
    {
        Description = "Remove all permanent mobs of the same type on the map.";
        _dataService = dataService;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        int mobId = paramsValue.Length > 0 ? int.Parse(paramsValue[0]) : -1;

        _dataService.RemovePLife(player, LifeType.Monster, mobId);
    }
}
