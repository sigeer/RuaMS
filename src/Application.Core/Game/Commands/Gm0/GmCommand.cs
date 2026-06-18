using Application.Core.Managers;

namespace Application.Core.Game.Commands.Gm0;


public class GmCommand : CommandBase
{
    public GmCommand() : base(0, "gm")
    {
        Description = "Send a message to the game masters.";
    }

    readonly static string[] tips = {
                "Please only use @gm in emergencies or to report somebody.",
                "To report a bug or make a suggestion, use the forum.",
                "Please do not use @gm to ask if a GM is online.",
                "Do not ask if you can receive help, just state your issue.",
                "Do not say 'I have a bug to report', just state it.",
        };
    public override async Task Execute(IChannelClient c, string[] paramValues)
    {
        var player = c.OnlinedCharacter;
        if (paramValues.Length < 1 || paramValues[0].Length < 3)
        { // #goodbye 'hi'
            await player.Pink("Your message was too short. Please provide as much detail as possible.");
            return;
        }
        string message = player.getLastCommandMessage();
        c.CurrentServer.NodeService.SendDropMessage(-1, "[GM Message]:" + CharacterManager.makeMapleReadable(player.getName()) + ": " + message, true);
        c.CurrentServer.NodeService.SendDropMessage(1, message, true);
        log.Information("{CharacterName}: {Message}", CharacterManager.makeMapleReadable(player.getName()), message);
        await player.Pink("Your message '" + message + "' was sent to GMs.");
        await player.Pink(tips[Randomizer.nextInt(tips.Length)]);
    }
}
