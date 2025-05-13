/*
    This file is part of the HeavenMS MapleStory NewServer
    Copyleft (L) 2016 - 2019 RonanLana

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/



using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Game.Skills;
using Application.Core.Game.TheWorld;
using client;
using Microsoft.Extensions.Logging;
using net.packet;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Ronan
 */
public class PlayerMapTransitionHandler : ChannelHandlerBase
{
    public PlayerMapTransitionHandler(IWorldChannel server, ILogger<ChannelHandlerBase> logger) : base(server, logger)
    {
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        chr.setMapTransitionComplete();

        int beaconid = chr.getBuffSource(BuffStat.HOMING_BEACON);
        if (beaconid != -1)
        {
            chr.cancelBuffStats(BuffStat.HOMING_BEACON);

            chr.sendPacket(PacketCreator.giveBuff(1, beaconid, new BuffStatValue(BuffStat.HOMING_BEACON, 0)));
        }

        if (!chr.isHidden())
        {  // thanks Lame (Conrad) for noticing hidden characters controlling mobs
            foreach (var mo in chr.getMap().getMonsters())
            {    // thanks BHB, IxianMace, Jefe for noticing several issues regarding mob statuses (such as freeze)
                Monster m = (Monster)mo;
                if (m.getSpawnEffect() == 0 || m.getHp() < m.getMaxHp())
                {     // avoid effect-spawning mobs
                    if (m.getController() == chr)
                    {
                        c.sendPacket(PacketCreator.stopControllingMonster(m.getObjectId()));
                        m.sendDestroyData(c);
                        m.aggroRemoveController();
                    }
                    else
                    {
                        m.sendDestroyData(c);
                    }

                    m.sendSpawnData(c);
                    m.aggroSwitchController(chr, false);
                }
            }
        }
    }
}