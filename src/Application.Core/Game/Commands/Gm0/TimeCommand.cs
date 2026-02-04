using Application.Resources.Messages;
using Humanizer;

namespace Application.Core.Game.Commands.Gm0;

public class TimeCommand : CommandBase
{
    public TimeCommand() : base(0, "time")
    {
    }

    public override void Execute(IChannelClient client, string[] paramsValue)
    {
        client.OnlinedCharacter.yellowMessage(
            client.CurrentCulture.GetMessageByKey(nameof(ClientMessage.ServerTime)) 
            + client.CurrentServer.Node.GetCurrentTimeDateTimeOffset().Humanize(culture: client.CurrentCulture.CultureInfo));
    }
}
