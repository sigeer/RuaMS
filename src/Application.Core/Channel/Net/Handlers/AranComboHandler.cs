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


using Application.Core.Game.Skills;

namespace Application.Core.Channel.Net.Handlers;

public class AranComboHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var player = c.OnlinedCharacter;
        int skillLevel = player.getSkillLevel(SkillFactory.GetSkillTrust(Aran.COMBO_ABILITY));
        if (player.JobModel.IsAran() && (skillLevel > 0 || player.JobModel.Id == JobId.LEGEND))
        {
            long currentTime = c.CurrentServer.getCurrentTime();
            short combo = player.getCombo();
            if ((currentTime - player.getLastCombo()) > 3000 && combo > 0)
            {
                combo = 0;
            }
            combo++;
            switch (combo)
            {
                case 10:
                case 20:
                case 30:
                case 40:
                case 50:
                case 60:
                case 70:
                case 80:
                case 90:
                case 100:
                    if (player.getJob().getId() != 2000 && (combo / 10) > skillLevel)
                    {
                        break;
                    }
                    SkillFactory.GetSkillTrust(Aran.COMBO_ABILITY).getEffect(combo / 10).applyComboBuff(player, combo);
                    break;
            }
            player.setCombo(combo);
            player.setLastCombo(currentTime);
        }
    }
}
