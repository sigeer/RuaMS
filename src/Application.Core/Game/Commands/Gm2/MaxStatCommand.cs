using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class MaxStatCommand : CommandBase
{
    public MaxStatCommand() : base(2, "maxstat")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        await c.OnlinedCharacter.MaxStat();
        await c.OnlinedCharacter.Yellow(nameof(ClientMessage.MaxStatCommand_Result));
    }
}
