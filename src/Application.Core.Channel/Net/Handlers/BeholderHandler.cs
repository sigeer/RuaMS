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


using Application.Core.Game.Maps.AnimatedObjects;

namespace Application.Core.Channel.Net.Handlers;



/**
 * @author BubblesDev
 */
public class BeholderHandler : ChannelHandlerBase
{
    //Summon Skills noobs

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        //Console.WriteLine(slea.ToString());
        var summons = c.OnlinedCharacter.getSummonsValues();
        int oid = p.readInt();
        Summon? summon = null;
        foreach (Summon sum in summons)
        {
            if (sum.getObjectId() == oid)
            {
                summon = sum;
            }
        }
        if (summon != null)
        {
            int skillId = p.readInt();
            if (skillId == DarkKnight.AURA_OF_BEHOLDER)
            {
                p.readShort(); //Not sure.
            }
            else if (skillId == DarkKnight.HEX_OF_BEHOLDER)
            {
                p.readByte(); //Not sure.
            }            //show to others here
        }
        else
        {
            c.OnlinedCharacter.clearSummons();
        }
    }
}
