using Application.Core.Channel.DataProviders;
using Application.Core.Channel.ServerData;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm1;

public class WhoDropsCommand : CommandBase
{
    readonly WzStringQueryService _wzManager;
    public WhoDropsCommand(WzStringQueryService wzManager) : base(1, "whodrops")
    {
        _wzManager = wzManager;
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.WhoDropsCommand_Syntax));
            return Task.CompletedTask;
        }

        if (c.tryacquireClient())
        {
            try
            {
                string searchString = player.getLastCommandMessage();
                string output = "";
                var items = _wzManager.FindItemIdByName(c, searchString).MatchedItems.Take(3);
                foreach (var data in items)
                {
                    output += "#b" + data.Name + "#k is dropped by:\r\n";

                    output += string.Join(", ", MonsterInformationProvider.getInstance().FindDropperNames(c, data.Id).Take(50));
                    output += "\r\n\r\n";
                }
                c.getAbstractPlayerInteraction().npcTalk(NpcId.MAPLE_ADMINISTRATOR, output);
            }
            finally
            {
                c.releaseClient();
            }
        }
        else
        {
            player.yellowMessage("Please wait a while for your request to be processed.");
        }
        return Task.CompletedTask;
    }
}
