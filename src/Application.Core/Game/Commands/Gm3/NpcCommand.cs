namespace Application.Core.Game.Commands.Gm3;

public class NpcCommand : CommandBase
{
    public NpcCommand() : base(3, "npc")
    {
        Description = "Spawn an NPC on your location.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !npc <npcid>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var npcId))
        {
            await player.Yellow("Syntax: !npc <npcid>");
            return;
        }

        await player.MapModel.SpawnNpc(npcId, player.getPosition());
    }
}
