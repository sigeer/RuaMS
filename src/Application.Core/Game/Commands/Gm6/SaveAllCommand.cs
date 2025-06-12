using net.server;
using tools;

namespace Application.Core.Game.Commands.Gm6;

public class SaveAllCommand : CommandBase
{
    public SaveAllCommand() : base(6, "saveall")
    {
        Description = "Save all characters.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var world in Server.getInstance().getWorlds())
        {
            foreach (var chr in world.getPlayerStorage().GetAllOnlinedPlayers())
            {
                chr.saveCharToDB();
            }
        }
        string message = player.getName() + " used !saveall.";
        c.CurrentServerContainer.BroadcastWorldGMPacket(PacketCreator.serverNotice(5, message));
        player.message("All players saved successfully.");
    }
}
