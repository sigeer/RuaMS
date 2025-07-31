using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm6;

public class SetGmLevelCommand : CommandBase
{
    readonly AdminService _adminService;
    public SetGmLevelCommand(AdminService adminService) : base(6, "setgmlevel")
    {
        Description = "Set GM level of a player.";
        _adminService = adminService;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            player.yellowMessage("Syntax: !setgmlevel <playername> <newlevel>");
            return;
        }

        if (!int.TryParse(paramsValue[1], out var newLevel))
        {
            player.yellowMessage("Syntax: !setgmlevel <playername> <newlevel>");
            return;
        }
        _adminService.SetGmLevel(c.OnlinedCharacter, paramsValue[0], newLevel);
    }
}
