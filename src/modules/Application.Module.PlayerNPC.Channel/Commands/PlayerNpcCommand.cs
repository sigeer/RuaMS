using Application.Core.Channel.Services;
using Application.Core.Client;
using Application.Core.Game.Commands;
using Application.Core.Game.Life;

namespace Application.Module.PlayerNPC.Channel.Commands.Gm4;

public class PlayerNpcCommand : CommandBase
{
    readonly PlayerNPCManager _service;
    public PlayerNpcCommand(PlayerNPCManager service) : base(4, "playernpc")
    {
        Description = "Spawn a player NPC of an online player.";
        _service = service;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !playernpc <playername> [honor]");
            return;
        }

        var targetPlayer = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (targetPlayer == null)
        {
            return;
        }

        if (paramsValue.Length == 1)
            _service.SpawnPlayerNPCHere(player.getMapId(), player.getPosition(), targetPlayer);
        else
            _service.SpawnPlayerNPCByHonor(targetPlayer);

        //if (!)
        //{
        //    player.dropMessage(5, "Could not deploy PlayerNPC. Either there's no room available here or depleted out scriptids to use.");
        //}
    }
}
