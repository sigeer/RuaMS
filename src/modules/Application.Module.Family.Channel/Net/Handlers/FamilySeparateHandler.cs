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


using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Module.Family.Channel.Models;
using Application.Module.Family.Channel.Net.Packets;
using Application.Shared.Net;
using Application.Utility.Exceptions;

namespace Application.Module.Family.Channel.Net.Handlers;

public class FamilySeparateHandler : ChannelHandlerBase
{
    readonly FamilyManager _familyManager;

    public FamilySeparateHandler(FamilyManager familyManager)
    {
        _familyManager = familyManager;
    }

    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        var oldFamily = _familyManager.GetFamilyByPlayerId(c.OnlinedCharacter.Id);
        if (oldFamily == null)
        {
            return Task.CompletedTask;
        }
        var chrFamilyEntry = oldFamily.getEntryByID(c.OnlinedCharacter.Id);
        if (chrFamilyEntry == null)
        {
            throw new BusinessFatalException();
        }

        FamilyEntry? forkOn = null;
        bool isSenior;
        if (p.available() > 0)
        {
            //packet 0x95 doesn't send id, since there is only one senior
            forkOn = oldFamily.getEntryByID(p.readInt());
            if (!chrFamilyEntry.isJunior(forkOn))
            {
                return Task.CompletedTask;            }
            isSenior = true;
        }
        else
        {
            forkOn = chrFamilyEntry;
            isSenior = false;
        }
        if (forkOn == null)
        {
            return Task.CompletedTask;
        }

        var senior = forkOn.getSenior();
        if (senior == null)
        {
            return Task.CompletedTask;
        }
        int levelDiff = Math.Abs(c.OnlinedCharacter.getLevel() - senior.Level);
        int cost = 2500 * levelDiff;
        cost += levelDiff * levelDiff;
        if (c.OnlinedCharacter.getMeso() < cost)
        {
            c.sendPacket(FamilyPacketCreator.sendFamilyMessage(isSenior ? 81 : 80, cost));
            return Task.CompletedTask;
        }
        _familyManager.Fork(forkOn.Id, cost);
        return Task.CompletedTask;
    }
}
