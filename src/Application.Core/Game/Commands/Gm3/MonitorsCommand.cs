using Application.Core.Channel.ServerData;

namespace Application.Core.Game.Commands.Gm3;
public class MonitorsCommand : CommandBase
{
    readonly MonitorManager _adminService;
    public MonitorsCommand(MonitorManager adminService) : base(3, "monitors")
    {
        Description = "Show all characters being monitored for packet logging";
        _adminService = adminService;
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var data = _adminService.GetMonitor();
        foreach (var item in data)
        {
            player.yellowMessage(item.Value + " is being monitored.");
        }
        return Task.CompletedTask;
    }
}
