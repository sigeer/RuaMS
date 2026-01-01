using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class MaxStatCommand : CommandBase
{
    public MaxStatCommand() : base(2, "maxstat")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        c.OnlinedCharacter.MaxStat();
        c.OnlinedCharacter.YellowMessageI18N(nameof(ClientMessage.MaxStatCommand_Result));
        return Task.CompletedTask;
    }
}
