namespace Application.Core.Game.Commands.Gm0;

public class ToggleExpCommand : CommandBase
{
    public ToggleExpCommand() : base(0, "toggleexp")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        if (c.tryacquireClient())
        {
            try
            {
                c.OnlinedCharacter.toggleExpGain();  // Vcoc's idea
            }
            finally
            {
                c.releaseClient();
            }
        }
        return Task.CompletedTask;
    }
}
