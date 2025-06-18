/*
	This file is part of the OdinMS Maple Story Server
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


using Application.Core.Channel;
using Application.Module.Family.Channel.Net.Packets;
using Application.Shared.Net;
using System.Collections.Concurrent;

namespace Application.Module.Family.Channel.Models;


/// <summary>
/// 
/// </summary>
public class Family
{
    public int Id { get; set; }
    public ConcurrentDictionary<int, FamilyEntry> Members { get; set; }

    private string? preceptsMessage = "";


    public WorldChannelServer Server { get; }
    public Family(WorldChannelServer serverContainer, int id)
    {
        Server = serverContainer;
        Id = id;
        Members = new();
    }

    public void UpdateMember(FamilyEntry member)
    {
        Members[member.Id] = member;
    }


    public FamilyEntry getLeader()
    {
        return Members.Values.FirstOrDefault(x => x.SeniorId == 0)!;
    }

    public int getTotalMembers()
    {
        return Members.Count;
    }

    public string getName()
    {
        return getLeader().Name;
    }

    public void setMessage(string? message, bool save)
    {
        preceptsMessage = message;
    }

    public string? getMessage()
    {
        return preceptsMessage;
    }

    public FamilyEntry? getEntryByID(int cid)
    {
        return Members.GetValueOrDefault(cid);
    }

    public void broadcast(Packet packet, int ignoreID = -1)
    {
        foreach (FamilyEntry entry in Members.Values)
        {
            var chr = Server.FindPlayerById(entry.Channel, entry.Id);
            if (chr != null)
            {
                if (chr.getId() == ignoreID)
                {
                    continue;
                }
                chr.sendPacket(packet);
            }
        }
    }

    public void broadcastFamilyInfoUpdate()
    {
        foreach (FamilyEntry entry in Members.Values)
        {
            var chr = Server.FindPlayerById(entry.Channel, entry.Id);
            if (chr != null)
            {
                chr.sendPacket(FamilyPacketCreator.getFamilyInfo(entry));
            }
        }
    }

}
