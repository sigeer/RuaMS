using Application.Core.Client;
using Application.Core.Game.TheWorld;
using constants.id;
using Microsoft.Extensions.Logging;
using net.packet;
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
    public RPSActionHandler(ILogger<ChannelHandlerBase> logger) : base(logger)
    {
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        var rps = chr.getRPS();

        if (c.tryacquireClient())
        {
            try
            {
                if (p.available() == 0 || !chr.getMap().containsNPC(NpcId.RPS_ADMIN))
                {
                    if (rps != null)
                    {
                        rps.dispose(c);
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
                            rps.reward(c);
                        }
                        if (chr.getMeso() >= 1000)
                        {
                            chr.setRPS(new RockPaperScissor(c, mode));
                        }
                        else
                        {
                            c.sendPacket(PacketCreator.rpsMesoError(-1));
                        }
                        break;
                    case 1: // answer
                        if (rps == null || !rps.answer(c, p.readByte()))
                        {
                            c.sendPacket(PacketCreator.rpsMode(0x0D));// 13
                        }
                        break;
                    case 2: // time over
                        if (rps == null || !rps.timeOut(c))
                        {
                            c.sendPacket(PacketCreator.rpsMode(0x0D));
                        }
                        break;
                    case 3: // continue
                        if (rps == null || !rps.nextRound(c))
                        {
                            c.sendPacket(PacketCreator.rpsMode(0x0D));
                        }
                        break;
                    case 4: // leave
                        if (rps != null)
                        {
                            rps.dispose(c);
                        }
                        else
                        {
                            c.sendPacket(PacketCreator.rpsMode(0x0D));
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
