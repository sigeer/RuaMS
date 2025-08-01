using Application.Core.Channel.DataProviders;

namespace Application.Core.Game.Commands.Gm1;

public class WhoDropsCommand : CommandBase
{
    public WhoDropsCommand() : base(1, "whodrops")
    {
        Description = "Show what drops an item.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.dropMessage(5, "Please do !whodrops <item name>");
            return;
        }

        if (c.tryacquireClient())
        {
            try
            {
                string searchString = player.getLastCommandMessage();
                string output = "";
                var items = ItemInformationProvider.getInstance().getItemDataByName(searchString).Take(3);
                foreach (var data in items)
                {
                    output += "#b" + data.Name + "#k is dropped by:\r\n";

                    output += string.Join(", ", MonsterInformationProvider.getInstance().FindDropperNames(data.Id).Take(50));
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
            player.dropMessage(5, "Please wait a while for your request to be processed.");
        }
    }
}
