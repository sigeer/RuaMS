using Application.Core.Managers;
using tools;

namespace Application.Core.Game.Commands.Gm0;

public class ReportBugCommand : CommandBase
{
    public ReportBugCommand() : base(0, "bug", "reportbug")
    {
        Description = "Send in a bug report.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        if (paramsValue.Length < 1)
        {
            player.dropMessage(5, $"Message too short and not sent. Please do {CurrentCommand} <bug>");
            return;
        }
        string message = player.getLastCommandMessage();
        await c.CurrentServerContainer.SendYellowTip("[Bug]:" + CharacterManager.makeMapleReadable(player.getName()) + ": " + message, true);
        await c.CurrentServerContainer.SendDropGMMessage(1, message);
        log.Information("{CharacterName}: {LastCommand}", CharacterManager.makeMapleReadable(player.getName()), message);
        player.dropMessage(5, "Your bug '" + message + "' was submitted successfully to our developers. Thank you!");

    }
}
