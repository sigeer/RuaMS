using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm4;

public class PmobCommand : CommandBase
{
    readonly DataService _dataService;
    public PmobCommand(DataService dataService) : base(4, "pmob")
    {
        Description = "Spawn a permanent mob on your location.";
        _dataService = dataService;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !pmob <mobid> [<mobtime>]");
            return;
        }

        // command suggestion thanks to HighKey21, none, bibiko94 (TAYAMO), asafgb
        int mapId = player.getMapId();
        int mobId = int.Parse(paramsValue[0]);
        int mobTime = (paramsValue.Length > 1) ? int.Parse(paramsValue[1]) : -1;

        await _dataService.CreatePLife(player, mobId, LifeType.Monster, mobTime);
    }
}
