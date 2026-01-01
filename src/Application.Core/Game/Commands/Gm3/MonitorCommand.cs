using Application.Core.Channel.ServerData;

namespace Application.Core.Game.Commands.Gm3;

public class MonitorCommand : CommandBase
{
    readonly MonitorManager _monitorManager;
    public MonitorCommand(MonitorManager adminService) : base(3, "monitor")
    {
        Description = "Toggle monitored packet logging of a character.";
        _monitorManager = adminService;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !monitor <ign>");
            return;
        }

       await  _monitorManager.ToggleMonitor(c.OnlinedCharacter, paramsValue[0]);
    }
}
