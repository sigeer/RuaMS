namespace Application.Core.Game.Commands.Gm0;

public class ToggleExpCommand : CommandBase
{
    public ToggleExpCommand() : base(0, "toggleexp")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        {
            await c.tryacquireClient();
            try
            {
                c.OnlinedCharacter.toggleExpGain();  // Vcoc's idea
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
