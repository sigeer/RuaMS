namespace Application.Core.Channel.Net.Handlers;

/**
 * @author kevintjuh93
 */
public class AdminChatHandler : ChannelHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        if (!c.OnlinedCharacter.isGM())
        {//if ( (signed int)CWvsContext::GetAdminLevel((void *)v294) > 2 )
            return;
        }
        byte mode = p.readByte();
        //not saving slides...
        string message = p.readString();
        var type = p.readByte();
        switch (mode)
        {
            case 0:
                {
                    // /alertall, /noticeall, /slideall
                    await c.CurrentServerContainer.SendDropMessage(type, message);
                    // ChatLogger.log(c, "Alert All", message);
                    break;
                }
            case 1:
                {
                    // /alertch, /noticech, /slidech
                    c.CurrentServer.dropMessage(type, message);
                    // ChatLogger.log(c, "Alert Ch", message);
                    break;
                }
            case 2:
                {
                    // /alertm /alertmap, /noticem /noticemap, /slidem /slidemap
                    c.OnlinedCharacter.getMap().dropMessage(type, message);
                    // ChatLogger.log(c, "Alert Map", message);
                    break;
                }
        }
    }
}
