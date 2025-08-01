using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm2;

public class SummonCommand : CommandBase
{
    readonly AdminService _adminService;
    public SummonCommand(AdminService adminService) : base(2, "warphere", "summon")
    {
        _adminService = adminService;
        Description = "Move a player to your location.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage($"Syntax: !{CurrentCommand} <playername>");
            return;
        }

        _adminService.SummonPlayerByName(c.OnlinedCharacter, paramsValue[0]);
    }
}
