namespace Application.Core.Game.Commands.Gm3;

public class StopMapEventCommand : CommandBase
{
    public StopMapEventCommand() : base(3, "stopmapevent")
    {
        Description = "Stop ongoing \"classic\" event.";
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        c.OnlinedCharacter.getMap().setEventStarted(false);
        return Task.CompletedTask;
    }
}
