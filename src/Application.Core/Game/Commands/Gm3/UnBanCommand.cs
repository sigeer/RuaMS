using Application.Core.Managers;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Game.Commands.Gm3;


public class UnBanCommand : CommandBase
{
    public UnBanCommand() : base(3, "unban")
    {
        Description = "Unban a player.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !unban <playername>");
            return;
        }

        try
        {
            c.CurrentServer.Transport.SendUnbanAccount(paramsValue[0]);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            player.message("Failed to unban " + paramsValue[0]);
            return;
        }
        player.message("Unbanned " + paramsValue[0]);
    }
}
