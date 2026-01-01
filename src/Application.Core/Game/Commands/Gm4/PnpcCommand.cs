using Application.Core.Channel.Services;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm4;

public class PnpcCommand : CommandBase
{
    readonly DataService _dataService;
    public PnpcCommand(DataService dataService) : base(4, "pnpc")
    {
        _dataService = dataService;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.PnpcCommand_Syntax));
            return;
        }

        if (!int.TryParse(paramsValue[0], out var npcId))
        {
            player.YellowMessageI18N(nameof(ClientMessage.DataTypeIncorrect), player.GetMessageByKey(nameof(ClientMessage.DataType_Number)));
            return;
        }

        // command suggestion thanks to HighKey21, none, bibiko94 (TAYAMO), asafgb
        if (player.getMap().containsNPC(npcId))
        {
            player.dropMessage(5, "This map already contains the specified NPC.");
            return;
        }

       await  _dataService.CreatePLife(player, npcId, LifeType.NPC);
    }
}
