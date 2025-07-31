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


using Application.Core.Channel.ServerData;
using client;
using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Net.Handlers;

public class TouchMonsterDamageHandler : AbstractDealDamageHandler
{
    public TouchMonsterDamageHandler(ILogger<AbstractDealDamageHandler> logger, AutoBanDataManager autoBanDataManager) : base(logger, autoBanDataManager)
    {
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        if (chr.getEnergyBar() == 15000 || chr.getBuffedValue(BuffStat.BODY_PRESSURE) != null)
        {
            applyAttack(parseDamage(p, chr, false, false), c.OnlinedCharacter, 1);
        }
    }
}
