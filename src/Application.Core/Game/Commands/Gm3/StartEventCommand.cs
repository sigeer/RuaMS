using Application.Core.Channel;
using Application.Resources.Messages;
using server.events.gm;

namespace Application.Core.Game.Commands.Gm3;

public class StartEventCommand : CommandBase
{
    public StartEventCommand() : base(3, "startevent")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        int players = 50;
        if (paramsValue.Length > 1)
        {
            players = int.Parse(paramsValue[0]);
        }
        c.getChannelServer().setEvent(new Event(player.getMapId(), players));

        var noticeMsg = string.Format(nameof(ClientMessage.StartEventCommand_Notice), ClientCulture.SystemCulture.GetMapName(player.getMap().Id), players.ToString());
        c.CurrentServerContainer.EarnTitleMessage(noticeMsg, false);
        c.CurrentServerContainer.SendDropMessage(6, noticeMsg);
    }
}
