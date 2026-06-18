using Application.Core.Managers;

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
            await player.Pink($"Message too short and not sent. Please do {CurrentCommand} <bug>");
            return;
        }
        string message = player.getLastCommandMessage();
        c.CurrentServer.NodeService.SendDropMessage(-1, "[Bug]:" + CharacterManager.makeMapleReadable(player.getName()) + ": " + message, true);
        c.CurrentServer.NodeService.SendDropMessage(1, message, true);
        log.Information("{CharacterName}: {LastCommand}", CharacterManager.makeMapleReadable(player.getName()), message);
        await player.Pink("Your bug '" + message + "' was submitted successfully to our developers. Thank you!");

    }
}
