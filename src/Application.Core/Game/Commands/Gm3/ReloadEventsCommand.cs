namespace Application.Core.Game.Commands.Gm3;

public class ReloadEventsCommand : CommandBase
{
    public ReloadEventsCommand() : base(3, "reloadevents")
    {
        Description = "Reload all event data.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        await c.CurrentServerContainer.SendReloadEvents(c.OnlinedCharacter);
    }
}
