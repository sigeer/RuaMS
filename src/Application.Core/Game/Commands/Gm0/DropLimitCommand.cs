using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm0;


public class DropLimitCommand : CommandBase
{
    public DropLimitCommand() : base(0, "droplimit")
    {

    }
    public override void Execute(IChannelClient c, string[] paramValues)
    {
        int dropCount = c.OnlinedCharacter.getMap().getDroppedItemCount();
        if (((float)dropCount) / YamlConfig.config.server.ITEM_LIMIT_ON_MAP < 0.75f)
        {
            c.OnlinedCharacter.showHint(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.DropLimitCommand_Message1)) 
                + " #b" + dropCount + "#k / #e" + YamlConfig.config.server.ITEM_LIMIT_ON_MAP + "#n", 300);
        }
        else
        {
            c.OnlinedCharacter.showHint(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.DropLimitCommand_Message1)) 
                + " #r" + dropCount + "#k / #e" + YamlConfig.config.server.ITEM_LIMIT_ON_MAP + "#n", 300);
        }

    }
}
