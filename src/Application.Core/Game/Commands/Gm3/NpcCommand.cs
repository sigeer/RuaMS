using server.life;
using tools;

namespace Application.Core.Game.Commands.Gm3;

public class NpcCommand : CommandBase
{
    public NpcCommand() : base(3, "npc")
    {
        Description = "Spawn an NPC on your location.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !npc <npcid>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var npcId))
        {
            player.yellowMessage("Syntax: !npc <npcid>");
            return;
        }

        player.MapModel.SpawnNpc(npcId, player.getPosition());
    }
}
