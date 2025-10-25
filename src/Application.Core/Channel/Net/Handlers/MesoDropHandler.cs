/*
	This file is part of the OdinMS Maple Story NewServer
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

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


using Application.Core.Channel.Services;
using Application.Resources.Messages;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Matze
 * @author Ronan - concurrency protection
 */
public class MesoDropHandler : ChannelHandlerBase
{
    readonly IFishingService _fishingService;

    public MesoDropHandler(IFishingService fishingService)
    {
        _fishingService = fishingService;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var player = c.OnlinedCharacter;
        if (!player.isAlive())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        p.skip(4);
        int meso = p.readInt();

        if (player.isGM() && player.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_DROP)
        {
            player.MessageI18N(nameof(ClientMessage.DropMeso_NotAccess));
            return;
        }

        if (c.tryacquireClient())
        {     // thanks imbee for noticing players not being able to throw mesos too fast
            try
            {
                if (meso <= player.getMeso() && meso > 9 && meso < 50001)
                {
                    player.gainMeso(-meso, false, true, false);
                }
                else
                {
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
        else
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }


        if (_fishingService.AttemptCatchFish(player, meso))
        {
            player.getMap().disappearingMesoDrop(meso, player, player, player.getPosition());
        }
        else
        {
            player.getMap().spawnMesoDrop(meso, player.getPosition(), player, player, true, DropType.FreeForAll);
        }
    }
}