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
using Application.Core.Game.TheWorld;
using Microsoft.Extensions.Logging;
using net.packet;

namespace Application.Core.Channel.Net.Handlers;

public class MobBanishPlayerHandler : ChannelHandlerBase
{
    public MobBanishPlayerHandler(IWorldChannel server, ILogger<ChannelHandlerBase> logger) : base(server, logger)
    {
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int mobId = p.readInt();     // mob banish handling detected thanks to MedicOP

        var chr = c.OnlinedCharacter;
        var mob = chr.getMap().getMonsterById(mobId);
        if (mob == null)
        {
            return;
        }

        var banishInfo = mob.getBanish();
        if (banishInfo == null)
        {
            return;
        }
        chr.changeMapBanish(banishInfo);
    }
}
