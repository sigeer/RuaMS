using server.minigame;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @Author Arnah
 * @Website http://Vertisy.ca/
 * @since Aug 15, 2016
 */
public class RPSActionHandler : ChannelHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        var rps = chr.getRPS();

        {
            await c.tryacquireClient();
            try
            {
                if (p.available() == 0 || !chr.getMap().containsNPC(NpcId.RPS_ADMIN))
                {
                    if (rps != null)
                    {
                        await rps.dispose(c);
                    }
                    return;
                }
                byte mode = p.readByte();
                switch (mode)
                {
                    case 0: // start game
                    case 5: // retry
                        if (rps != null)
                        {
                            await rps.reward(c);
                        }
                        if (chr.getMeso() >= 1000)
                        {
                            var o = new RockPaperScissor(c, mode);
                            await o.Initialize(c);
                            chr.setRPS(o);
                        }
                        else
                        {
                            await c.SendPacket(PacketCreator.rpsMesoError(-1));
                        }
                        break;
                    case 1: // answer
                        if (rps == null || !await rps.answer(c, p.readByte()))
                        {
                            await c.SendPacket(PacketCreator.rpsMode(0x0D));// 13
                        }
                        break;
                    case 2: // time over
                        if (rps == null || !await rps.timeOut(c))
                        {
                            await c.SendPacket(PacketCreator.rpsMode(0x0D));
                        }
                        break;
                    case 3: // continue
                        if (rps == null || !await rps.nextRound(c))
                        {
                            await c.SendPacket(PacketCreator.rpsMode(0x0D));
                        }
                        break;
                    case 4: // leave
                        if (rps != null)
                        {
                            await rps.dispose(c);
                        }
                        else
                        {
                            await c.SendPacket(PacketCreator.rpsMode(0x0D));
                        }
                        break;
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
